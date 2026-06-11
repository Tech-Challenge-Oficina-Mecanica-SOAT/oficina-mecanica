terraform {
  required_providers {
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = "~> 1.14"
    }
  }
}

variable "postgres_password" {
  description = "Senha do Postgres"
  type        = string
  sensitive   = true
}

variable "storage_size" {
  description = "Tamanho do PVC do Postgres"
  type        = string
  default     = "5Gi"
}

# ─── PVC ─────────────────────────────────────────────────────
resource "kubectl_manifest" "postgres_pvc" {
  yaml_body = <<-YAML
    apiVersion: v1
    kind: PersistentVolumeClaim
    metadata:
      name: postgres-pvc
    spec:
      accessModes:
        - ReadWriteOnce
      resources:
        requests:
          storage: ${var.storage_size}
  YAML
}

# ─── Deployment ──────────────────────────────────────────────
resource "kubectl_manifest" "postgres_deployment" {
  yaml_body  = file("${path.module}/../../../k8s/postgres-deployment.yaml")
  depends_on = [kubectl_manifest.postgres_pvc]
}

# ─── Service ─────────────────────────────────────────────────
resource "kubectl_manifest" "postgres_service" {
  yaml_body  = file("${path.module}/../../../k8s/postgres-service.yaml")
  depends_on = [kubectl_manifest.postgres_deployment]
}

output "postgres_service_name" {
  description = "Nome do service do Postgres dentro do cluster"
  value       = "postgres"
}

output "postgres_port" {
  description = "Porta do Postgres"
  value       = 5432
}
