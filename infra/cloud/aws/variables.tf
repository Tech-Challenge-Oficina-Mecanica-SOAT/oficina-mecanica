variable "aws_region" {
  description = "Região AWS onde o cluster será provisionado"
  type        = string
  default     = "us-east-1"
}

variable "cluster_name" {
  description = "Nome do cluster EKS"
  type        = string
  default     = "oficina-mecanica"
}

variable "cluster_version" {
  description = "Versão do Kubernetes no EKS"
  type        = string
  default     = "1.29"
}

variable "node_instance_type" {
  description = "Tipo de instância EC2 para os worker nodes"
  type        = string
  default     = "t3.small"
}

variable "node_min_size" {
  description = "Número mínimo de nodes no node group"
  type        = number
  default     = 1
}

variable "node_max_size" {
  description = "Número máximo de nodes no node group"
  type        = number
  default     = 3
}

variable "node_desired_size" {
  description = "Número desejado de nodes no node group"
  type        = number
  default     = 2
}

variable "db_instance_class" {
  description = "Classe de instância RDS para o PostgreSQL"
  type        = string
  default     = "db.t3.micro"
}

variable "db_name" {
  description = "Nome do banco de dados"
  type        = string
  default     = "OficinaDB"
}

variable "db_username" {
  description = "Usuário master do PostgreSQL"
  type        = string
  default     = "postgres"
  sensitive   = true
}

variable "db_password" {
  description = "Senha master do PostgreSQL (forneça via TF_VAR_db_password ou terraform.tfvars)"
  type        = string
  sensitive   = true
}

variable "tags" {
  description = "Tags comuns aplicadas a todos os recursos"
  type        = map(string)
  default = {
    Project     = "oficina-mecanica"
    Environment = "production"
    ManagedBy   = "terraform"
  }
}
