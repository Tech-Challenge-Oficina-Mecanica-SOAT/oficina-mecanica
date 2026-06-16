output "cluster_name" {
  description = "Nome do cluster EKS criado"
  value       = module.eks.cluster_name
}

output "cluster_endpoint" {
  description = "Endpoint do API server do EKS"
  value       = module.eks.cluster_endpoint
}

output "configure_kubectl" {
  description = "Comando para configurar o kubectl apontando para o cluster"
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${module.eks.cluster_name}"
}

output "rds_endpoint" {
  description = "Endpoint do banco PostgreSQL no RDS"
  value       = aws_db_instance.postgres.address
  sensitive   = true
}

output "rds_connection_string" {
  description = "Connection string do PostgreSQL para uso no Kubernetes Secret"
  value       = "Host=${aws_db_instance.postgres.address};Database=${var.db_name};Username=${var.db_username};Password=${var.db_password};Trust Server Certificate=true"
  sensitive   = true
}
