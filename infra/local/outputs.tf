output "cluster_name" {
  description = "Nome do cluster Kind criado"
  value       = kind_cluster.oficina.name
}

output "cluster_endpoint" {
  description = "Endpoint do cluster Kind"
  value       = kind_cluster.oficina.endpoint
}

output "kubeconfig_path" {
  description = "Caminho do kubeconfig gerado pelo Kind"
  value       = "~/.kube/config"
}
