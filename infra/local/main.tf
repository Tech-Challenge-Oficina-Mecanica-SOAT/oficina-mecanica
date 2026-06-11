terraform {
  required_version = ">= 1.6.0"

  required_providers {
    kind = {
      source  = "tehcyx/kind"
      version = "~> 0.4"
    }
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = "~> 1.14"
    }
  }
}

provider "kind" {}

provider "kubectl" {
  host                   = kind_cluster.oficina.endpoint
  cluster_ca_certificate = kind_cluster.oficina.cluster_ca_certificate
  client_certificate     = kind_cluster.oficina.client_certificate
  client_key             = kind_cluster.oficina.client_key
  load_config_file       = false
}

# ─── Cluster Kind ────────────────────────────────────────────
resource "kind_cluster" "oficina" {
  name           = "oficina-mecanica"
  wait_for_ready = true

  kind_config {
    kind        = "Cluster"
    api_version = "kind.x-k8s.io/v1alpha4"

    node {
      role = "control-plane"
    }

    node {
      role = "worker"
    }
  }
}

# ─── Manifestos Kubernetes ───────────────────────────────────

resource "kubectl_manifest" "secret" {
  yaml_body  = file("${path.module}/../../k8s/secret.yaml")
  depends_on = [kind_cluster.oficina]
}

resource "kubectl_manifest" "configmap" {
  yaml_body  = file("${path.module}/../../k8s/configmap.yaml")
  depends_on = [kind_cluster.oficina]
}

resource "kubectl_manifest" "postgres_pvc" {
  yaml_body  = file("${path.module}/../../k8s/postgres-pvc.yaml")
  depends_on = [kind_cluster.oficina]
}

resource "kubectl_manifest" "postgres_deployment" {
  yaml_body  = file("${path.module}/../../k8s/postgres-deployment.yaml")
  depends_on = [kubectl_manifest.postgres_pvc, kubectl_manifest.secret]
}

resource "kubectl_manifest" "postgres_service" {
  yaml_body  = file("${path.module}/../../k8s/postgres-service.yaml")
  depends_on = [kubectl_manifest.postgres_deployment]
}

resource "kubectl_manifest" "mailhog" {
  yaml_body  = file("${path.module}/../../k8s/mailhog-deployment.yaml")
  depends_on = [kind_cluster.oficina]
}

resource "kubectl_manifest" "api_deployment" {
  yaml_body  = file("${path.module}/../../k8s/api-deployment.yaml")
  depends_on = [kubectl_manifest.postgres_service, kubectl_manifest.configmap, kubectl_manifest.secret]
}

resource "kubectl_manifest" "api_service" {
  yaml_body  = file("${path.module}/../../k8s/api-service.yaml")
  depends_on = [kubectl_manifest.api_deployment]
}

resource "kubectl_manifest" "api_hpa" {
  yaml_body  = file("${path.module}/../../k8s/api-hpa.yaml")
  depends_on = [kubectl_manifest.api_deployment]
}
