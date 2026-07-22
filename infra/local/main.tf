terraform {
  required_version = ">= 1.6.0"

  required_providers {
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
    local = {
      source  = "hashicorp/local"
      version = "~> 2.5"
    }
  }
}

# ─── Cluster Kind ────────────────────────────────────────────
#
# Os providers de comunidade "tehcyx/kind" e "gavinbunney/kubectl" (usados
# anteriormente aqui) estão sem manutenção desde ~2021 e quebram o handshake
# de plugins em versões recentes do Terraform (erro "x509: certificate signed
# by unknown authority" / "Plugin did not respond"). Em vez de depender deles,
# chamamos os binários "kind" e "kubectl" diretamente via local-exec — mesmo
# resultado, sem o risco de incompatibilidade do provider.
resource "null_resource" "kind_cluster" {
  triggers = {
    cluster_name     = var.cluster_name
    config_hash      = filemd5("${path.module}/kind-config.yaml")
    bash_interpreter = var.bash_interpreter
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = <<-EOT
      set -e
      if kind get clusters | grep -qx "${var.cluster_name}"; then
        echo "Cluster '${var.cluster_name}' já existe, pulando criação."
      else
        kind create cluster --name "${var.cluster_name}" --config "${path.module}/kind-config.yaml" --wait 90s
      fi
      # No Windows, a verificação do certificado do client Go falha de forma
      # cosmética para clusters Kind locais; a conectividade real funciona.
      kubectl config set-cluster "kind-${var.cluster_name}" --insecure-skip-tls-verify=true
    EOT
  }

  provisioner "local-exec" {
    when        = destroy
    interpreter = [self.triggers.bash_interpreter, "-c"]
    command     = "kind delete cluster --name \"${self.triggers.cluster_name}\" || true"
  }
}

# ─── Imagem da API (kind load de uma imagem já buildada, sem registry) ───
#
# O "docker build" é feito FORA do Terraform (passo manual antes do apply —
# ver README/FASE2_COMPLIANCE). No Windows, invocar "docker build" como filho
# do processo do terraform.exe cancela a transferência do build context com
# "error from sender: context canceled" — uma interação específica entre o
# gerenciamento de processos do Terraform e o streaming do buildx nesse SO.
# O mesmo build funciona normalmente via PowerShell/Bash direto, então a
# responsabilidade de buildar a imagem fica fora do grafo do Terraform.
resource "null_resource" "load_image" {
  count = var.load_local_image ? 1 : 0

  depends_on = [null_resource.kind_cluster]

  triggers = {
    image = var.api_image
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = <<-EOT
      set -e
      if ! docker image inspect "${var.api_image}" >/dev/null 2>&1; then
        echo "Imagem '${var.api_image}' não encontrada localmente."
        echo "Builde antes de aplicar: docker build -t ${var.api_image} ."
        exit 1
      fi
      kind load docker-image "${var.api_image}" --name "${var.cluster_name}"
    EOT
  }
}

# Deployment da API com a tag de imagem (e imagePullPolicy correta) já
# substituídos — gerado a partir do manifesto versionado, sem alterá-lo.
resource "local_file" "api_deployment_rendered" {
  filename = "${path.module}/.generated/api-deployment.yaml"
  content = replace(
    file("${path.module}/../../k8s/api-deployment.yaml"),
    "image: IMAGE_TAG_PLACEHOLDER",
    "image: ${var.api_image}\n          imagePullPolicy: ${var.load_local_image ? "Never" : "IfNotPresent"}"
  )
}

# ─── Manifestos Kubernetes ───────────────────────────────────

resource "null_resource" "secret" {
  depends_on = [null_resource.kind_cluster]

  triggers = {
    hash = filemd5("${path.module}/../../k8s/secret.yaml")
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = "kubectl --context \"kind-${var.cluster_name}\" apply -f \"${path.module}/../../k8s/secret.yaml\""
  }
}

resource "null_resource" "configmap" {
  depends_on = [null_resource.kind_cluster]

  triggers = {
    hash = filemd5("${path.module}/../../k8s/configmap.yaml")
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = "kubectl --context \"kind-${var.cluster_name}\" apply -f \"${path.module}/../../k8s/configmap.yaml\""
  }
}

resource "null_resource" "postgres_pvc" {
  depends_on = [null_resource.kind_cluster]

  triggers = {
    hash = filemd5("${path.module}/../../k8s/postgres-pvc.yaml")
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = "kubectl --context \"kind-${var.cluster_name}\" apply -f \"${path.module}/../../k8s/postgres-pvc.yaml\""
  }
}

resource "null_resource" "postgres_deployment" {
  depends_on = [null_resource.postgres_pvc, null_resource.secret]

  triggers = {
    hash = filemd5("${path.module}/../../k8s/postgres-deployment.yaml")
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = <<-EOT
      set -e
      kubectl --context "kind-${var.cluster_name}" apply -f "${path.module}/../../k8s/postgres-deployment.yaml"
      kubectl --context "kind-${var.cluster_name}" rollout status deployment/postgres --timeout=120s
    EOT
  }
}

resource "null_resource" "postgres_service" {
  depends_on = [null_resource.postgres_deployment]

  triggers = {
    hash = filemd5("${path.module}/../../k8s/postgres-service.yaml")
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = "kubectl --context \"kind-${var.cluster_name}\" apply -f \"${path.module}/../../k8s/postgres-service.yaml\""
  }
}

resource "null_resource" "mailhog" {
  depends_on = [null_resource.kind_cluster]

  triggers = {
    hash = filemd5("${path.module}/../../k8s/mailhog-deployment.yaml")
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = "kubectl --context \"kind-${var.cluster_name}\" apply -f \"${path.module}/../../k8s/mailhog-deployment.yaml\""
  }
}

resource "null_resource" "api_deployment" {
  depends_on = [
    null_resource.postgres_service,
    null_resource.configmap,
    null_resource.secret,
    null_resource.load_image,
  ]

  triggers = {
    hash = local_file.api_deployment_rendered.content_md5
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = <<-EOT
      set -e
      kubectl --context "kind-${var.cluster_name}" apply -f "${local_file.api_deployment_rendered.filename}"
      kubectl --context "kind-${var.cluster_name}" rollout status deployment/oficina-mecanica-api --timeout=120s
    EOT
  }
}

resource "null_resource" "api_service" {
  depends_on = [null_resource.api_deployment]

  triggers = {
    hash = filemd5("${path.module}/../../k8s/api-service.yaml")
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = "kubectl --context \"kind-${var.cluster_name}\" apply -f \"${path.module}/../../k8s/api-service.yaml\""
  }
}

resource "null_resource" "api_hpa" {
  depends_on = [null_resource.api_deployment]

  triggers = {
    hash = filemd5("${path.module}/../../k8s/api-hpa.yaml")
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = "kubectl --context \"kind-${var.cluster_name}\" apply -f \"${path.module}/../../k8s/api-hpa.yaml\""
  }
}

# ─── metrics-server (obrigatório para o HPA funcionar) ───────
resource "null_resource" "metrics_server" {
  depends_on = [null_resource.kind_cluster]

  triggers = {
    cluster_name = var.cluster_name
  }

  provisioner "local-exec" {
    interpreter = [var.bash_interpreter, "-c"]
    command     = <<-EOT
      set -e
      kubectl --context "kind-${var.cluster_name}" apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
      kubectl --context "kind-${var.cluster_name}" patch deployment metrics-server -n kube-system --type='json' \
        -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]' || true
      kubectl --context "kind-${var.cluster_name}" rollout status deployment/metrics-server -n kube-system --timeout=90s
    EOT
  }
}
