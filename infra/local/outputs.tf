output "cluster_name" {
  description = "Nome do cluster Kind criado"
  value       = var.cluster_name
}

output "kubeconfig_context" {
  description = "Contexto kubectl apontando para o cluster (use com --context ou 'kubectl config use-context')"
  value       = "kind-${var.cluster_name}"
}

output "port_forward_api" {
  description = "Comando para expor a API localmente"
  value       = "kubectl --context kind-${var.cluster_name} port-forward svc/oficina-mecanica-api 5000:80"
}

output "port_forward_mailhog" {
  description = "Comando para expor o MailHog localmente"
  value       = "kubectl --context kind-${var.cluster_name} port-forward svc/mailhog 8025:8025"
}
