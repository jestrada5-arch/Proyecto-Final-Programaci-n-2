CREATE DATABASE  IF NOT EXISTS `db_punto_venta` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `db_punto_venta`;
-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: localhost    Database: db_punto_venta
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `clientes`
--

DROP TABLE IF EXISTS `clientes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientes` (
  `id_cliente` int NOT NULL AUTO_INCREMENT,
  `nombres` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `apellidos` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nit` varchar(12) COLLATE utf8mb4_unicode_ci NOT NULL,
  `genero` tinyint(1) NOT NULL,
  `telefono` varchar(25) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `correo_electronico` varchar(45) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `fecha_ingreso` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_cliente`),
  UNIQUE KEY `uq_clientes_nit` (`nit`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clientes`
--

LOCK TABLES `clientes` WRITE;
/*!40000 ALTER TABLE `clientes` DISABLE KEYS */;
INSERT INTO `clientes` VALUES (1,'Ana','Lopez','123456-7',0,'55551234','ana@correo.com','2026-10-20 20:05:02'),(2,'Luis','Ramirez','765432-1',1,'55112233','luis@correo.com','2026-10-23 19:47:49'),(3,'Prueba','nit','812345-1',1,'32278923','pruebanit@gmail.com','2026-10-25 04:21:22');
/*!40000 ALTER TABLE `clientes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `compras`
--

DROP TABLE IF EXISTS `compras`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `compras` (
  `id_compra` int NOT NULL AUTO_INCREMENT,
  `no_orden_compra` int NOT NULL,
  `id_proveedor` int NOT NULL,
  `id_usuario` int DEFAULT NULL,
  `fecha_orden` date NOT NULL,
  `fecha_ingreso` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_compra`),
  UNIQUE KEY `uq_compras_orden` (`no_orden_compra`),
  KEY `fk_compra_proveedor` (`id_proveedor`),
  KEY `fk_compra_usuario` (`id_usuario`),
  CONSTRAINT `fk_compra_proveedor` FOREIGN KEY (`id_proveedor`) REFERENCES `proveedores` (`id_proveedor`),
  CONSTRAINT `fk_compra_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `compras`
--

LOCK TABLES `compras` WRITE;
/*!40000 ALTER TABLE `compras` DISABLE KEYS */;
INSERT INTO `compras` VALUES (1,1,1,NULL,'2026-10-23','2026-10-23 21:23:49'),(2,2,2,NULL,'2026-10-23','2026-10-23 21:25:00'),(3,3,2,NULL,'2026-10-25','2026-10-25 04:17:53');
/*!40000 ALTER TABLE `compras` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `compras_detalle`
--

DROP TABLE IF EXISTS `compras_detalle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `compras_detalle` (
  `id_compra_detalle` bigint NOT NULL AUTO_INCREMENT,
  `id_compra` int NOT NULL,
  `id_producto` int NOT NULL,
  `cantidad` int NOT NULL,
  `precio_costo_unitario` decimal(8,2) NOT NULL,
  PRIMARY KEY (`id_compra_detalle`),
  KEY `fk_detalle_compra` (`id_compra`),
  KEY `fk_detalle_compra_producto` (`id_producto`),
  CONSTRAINT `fk_detalle_compra` FOREIGN KEY (`id_compra`) REFERENCES `compras` (`id_compra`) ON DELETE CASCADE,
  CONSTRAINT `fk_detalle_compra_producto` FOREIGN KEY (`id_producto`) REFERENCES `productos` (`id_producto`),
  CONSTRAINT `ck_cdet_cant` CHECK ((`cantidad` > 0)),
  CONSTRAINT `ck_cdet_precio` CHECK ((`precio_costo_unitario` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `compras_detalle`
--

LOCK TABLES `compras_detalle` WRITE;
/*!40000 ALTER TABLE `compras_detalle` DISABLE KEYS */;
INSERT INTO `compras_detalle` VALUES (1,1,2,10,13.00),(6,2,5,5,22.02),(7,2,4,1,10.09),(8,3,4,5,10.09);
/*!40000 ALTER TABLE `compras_detalle` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `empleados`
--

DROP TABLE IF EXISTS `empleados`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `empleados` (
  `id_empleado` int NOT NULL AUTO_INCREMENT,
  `nombres` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `apellidos` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `direccion` varchar(80) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `telefono` varchar(25) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `dpi` varchar(15) COLLATE utf8mb4_unicode_ci NOT NULL,
  `genero` tinyint(1) NOT NULL,
  `fecha_nacimiento` date DEFAULT NULL,
  `id_puesto` smallint NOT NULL,
  `fecha_inicio_labores` date DEFAULT NULL,
  `fecha_ingreso` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_empleado`),
  UNIQUE KEY `uq_empleados_dpi` (`dpi`),
  KEY `fk_empleado_puesto` (`id_puesto`),
  CONSTRAINT `fk_empleado_puesto` FOREIGN KEY (`id_puesto`) REFERENCES `puestos` (`id_puesto`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `empleados`
--

LOCK TABLES `empleados` WRITE;
/*!40000 ALTER TABLE `empleados` DISABLE KEYS */;
INSERT INTO `empleados` VALUES (1,'Carlos','Mendez','Zona 7, Guatemala','55443322','1234567890101',1,'1998-05-14',1,'2026-01-10','2026-10-23 19:47:49'),(2,'Maria','Gonzalez','Zona 11, Guatemala','55998877','2234567890102',0,'1995-03-22',2,'2026-02-01','2026-10-23 19:47:49');
/*!40000 ALTER TABLE `empleados` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `marcas`
--

DROP TABLE IF EXISTS `marcas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `marcas` (
  `id_marca` smallint NOT NULL AUTO_INCREMENT,
  `marca` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id_marca`),
  UNIQUE KEY `uq_marcas_marca` (`marca`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `marcas`
--

LOCK TABLES `marcas` WRITE;
/*!40000 ALTER TABLE `marcas` DISABLE KEYS */;
INSERT INTO `marcas` VALUES (3,'Alpina'),(2,'Bimbo'),(1,'Coca-Cola'),(4,'Colgate'),(5,'Pantene');
/*!40000 ALTER TABLE `marcas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `menu`
--

DROP TABLE IF EXISTS `menu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `menu` (
  `id_menu` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `url` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `icono` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `id_padre` int DEFAULT NULL,
  `orden` int DEFAULT '0',
  PRIMARY KEY (`id_menu`),
  KEY `fk_menu_padre` (`id_padre`),
  CONSTRAINT `fk_menu_padre` FOREIGN KEY (`id_padre`) REFERENCES `menu` (`id_menu`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `menu`
--

LOCK TABLES `menu` WRITE;
/*!40000 ALTER TABLE `menu` DISABLE KEYS */;
INSERT INTO `menu` VALUES (1,'Productos','/productos','bi-box-seam',NULL,1),(2,'Ventas','/ventas','bi-cart-check',NULL,2),(3,'Compras','/compras','bi-truck',NULL,3),(4,'Reportes','/reportes','bi-bar-chart',NULL,4),(5,'Marcas','/marcas','bi-tags',1,1),(6,'Clientes','/clientes','bi-people',2,1),(7,'Empleados','/empleados','bi-person-badge',2,2),(8,'Puestos','/puestos','bi-briefcase',7,1),(9,'Proveedores','/proveedores','bi-building',3,1),(11,'Productos','/reportes/productos','bi bi-box-seam',4,1),(12,'Existencias','/reportes/existencias','bi bi-boxes',4,2),(13,'Compras','/reportes/compras','bi bi-truck',4,3),(14,'Ventas','/reportes/ventas','bi bi-cash-coin',4,4);
/*!40000 ALTER TABLE `menu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `menu_roles`
--

DROP TABLE IF EXISTS `menu_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `menu_roles` (
  `id_menu` int NOT NULL,
  `rol` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id_menu`,`rol`),
  CONSTRAINT `fk_menu_roles_menu` FOREIGN KEY (`id_menu`) REFERENCES `menu` (`id_menu`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `menu_roles`
--

LOCK TABLES `menu_roles` WRITE;
/*!40000 ALTER TABLE `menu_roles` DISABLE KEYS */;
INSERT INTO `menu_roles` VALUES (1,'Bodeguero'),(1,'Cajero'),(2,'Cajero'),(3,'Bodeguero'),(4,'Bodeguero'),(4,'Cajero'),(6,'Cajero'),(9,'Bodeguero'),(12,'Bodeguero'),(13,'Bodeguero'),(14,'Cajero');
/*!40000 ALTER TABLE `menu_roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `productos`
--

DROP TABLE IF EXISTS `productos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `productos` (
  `id_producto` int NOT NULL AUTO_INCREMENT,
  `producto` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `id_marca` smallint NOT NULL,
  `descripcion` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `imagen` varchar(150) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `precio_costo` decimal(8,2) NOT NULL,
  `precio_venta` decimal(8,2) NOT NULL,
  `existencia` int NOT NULL,
  `fecha_ingreso` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_producto`),
  KEY `fk_producto_marca` (`id_marca`),
  CONSTRAINT `fk_producto_marca` FOREIGN KEY (`id_marca`) REFERENCES `marcas` (`id_marca`),
  CONSTRAINT `ck_prod_costo` CHECK ((`precio_costo` > 0)),
  CONSTRAINT `ck_prod_exist` CHECK ((`existencia` >= 0)),
  CONSTRAINT `ck_prod_venta` CHECK ((`precio_venta` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `productos`
--

LOCK TABLES `productos` WRITE;
/*!40000 ALTER TABLE `productos` DISABLE KEYS */;
INSERT INTO `productos` VALUES (1,'Coca-Cola 600ml',1,'Bebida gaseosa','/images/productos/prod_1_20261024205026.webp',4.50,6.00,43,'2026-10-23 19:47:49'),(2,'Pan Blanco Bimbo',2,'Pan de caja grande','/images/productos/prod_2_20261024205036.webp',13.00,16.25,35,'2026-10-23 19:47:49'),(3,'Yogurt Alpina 1L',3,'Yogurt natural','/images/productos/prod_3_20261024205100.webp',18.00,24.00,17,'2026-10-23 19:47:49'),(4,'Pasta Dental Colgade',4,'Pasta dental 100ml','/images/productos/prod_4_20261024205046.webp',10.09,12.61,3,'2026-10-23 19:47:49'),(5,'Shampoo Pantene 400ml',5,'Shampoo hidratante','/images/productos/prod_5_20261024205053.webp',22.02,27.52,27,'2026-10-23 19:47:49');
/*!40000 ALTER TABLE `productos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `proveedores`
--

DROP TABLE IF EXISTS `proveedores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `proveedores` (
  `id_proveedor` int NOT NULL AUTO_INCREMENT,
  `proveedor` varchar(60) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nit` varchar(12) COLLATE utf8mb4_unicode_ci NOT NULL,
  `direccion` varchar(80) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `telefono` varchar(25) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id_proveedor`),
  UNIQUE KEY `uq_proveedores_nit` (`nit`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `proveedores`
--

LOCK TABLES `proveedores` WRITE;
/*!40000 ALTER TABLE `proveedores` DISABLE KEYS */;
INSERT INTO `proveedores` VALUES (1,'Distribuidora La Central','1234567-8','5a avenida 10-20 zona 1, Guatemala','22334455'),(2,'Comercial del Norte','8765432-1','Km 15 Carretera al Atlántico','55667788');
/*!40000 ALTER TABLE `proveedores` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `puestos`
--

DROP TABLE IF EXISTS `puestos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `puestos` (
  `id_puesto` smallint NOT NULL AUTO_INCREMENT,
  `puesto` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id_puesto`),
  UNIQUE KEY `uq_puestos_puesto` (`puesto`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `puestos`
--

LOCK TABLES `puestos` WRITE;
/*!40000 ALTER TABLE `puestos` DISABLE KEYS */;
INSERT INTO `puestos` VALUES (2,'Bodeguero'),(1,'Cajero'),(3,'Supervisor');
/*!40000 ALTER TABLE `puestos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `id_usuario` int NOT NULL AUTO_INCREMENT,
  `usuario` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `password` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `id_empleado` int DEFAULT NULL,
  `fecha_ingreso` datetime DEFAULT CURRENT_TIMESTAMP,
  `rol` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Cajero',
  PRIMARY KEY (`id_usuario`),
  UNIQUE KEY `usuario` (`usuario`),
  KEY `fk_usuario_empleado` (`id_empleado`),
  CONSTRAINT `fk_usuario_empleado` FOREIGN KEY (`id_empleado`) REFERENCES `empleados` (`id_empleado`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'admin','$2a$11$3kmmZ1LeHJmbpD/2KsFjOuWkiGZJSe57vXWSs9O/EqSJHm5hexLSm',NULL,'2026-10-20 19:55:21','Administrador'),(2,'carlos.cajero','$2a$11$72lxh7Q6UnB9FR41Vfupf.4bmZepqxe/UWJRrT9KGv4mNLLeYie8y',1,'2026-10-25 04:08:52','Cajero'),(3,'maria.bodega','$2a$11$7plz9Cwjgxdm.Q0DECLih./FlEz99VkEKp.51q1/WynutKLRh2UH6',2,'2026-10-25 04:09:13','Bodeguero');
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ventas`
--

DROP TABLE IF EXISTS `ventas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ventas` (
  `id_venta` int NOT NULL AUTO_INCREMENT,
  `no_factura` int NOT NULL,
  `serie` char(1) COLLATE utf8mb4_unicode_ci NOT NULL,
  `fecha_factura` date NOT NULL,
  `id_cliente` int NOT NULL,
  `id_empleado` int NOT NULL,
  `fecha_ingreso` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_venta`),
  UNIQUE KEY `uq_ventas_factura` (`serie`,`no_factura`),
  KEY `fk_venta_cliente` (`id_cliente`),
  KEY `fk_venta_empleado` (`id_empleado`),
  CONSTRAINT `fk_venta_cliente` FOREIGN KEY (`id_cliente`) REFERENCES `clientes` (`id_cliente`),
  CONSTRAINT `fk_venta_empleado` FOREIGN KEY (`id_empleado`) REFERENCES `empleados` (`id_empleado`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ventas`
--

LOCK TABLES `ventas` WRITE;
/*!40000 ALTER TABLE `ventas` DISABLE KEYS */;
INSERT INTO `ventas` VALUES (2,1,'A','2026-10-25',1,1,'2026-10-25 04:13:56'),(3,2,'A','2026-10-25',3,1,'2026-10-25 04:23:14'),(4,3,'A','2026-10-25',1,2,'2026-10-25 04:28:09');
/*!40000 ALTER TABLE `ventas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ventas_detalle`
--

DROP TABLE IF EXISTS `ventas_detalle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ventas_detalle` (
  `id_venta_detalle` bigint NOT NULL AUTO_INCREMENT,
  `id_venta` int NOT NULL,
  `id_producto` int NOT NULL,
  `cantidad` int NOT NULL,
  `precio_unitario` decimal(8,2) NOT NULL,
  PRIMARY KEY (`id_venta_detalle`),
  KEY `fk_detalle_venta` (`id_venta`),
  KEY `fk_detalle_venta_producto` (`id_producto`),
  CONSTRAINT `fk_detalle_venta` FOREIGN KEY (`id_venta`) REFERENCES `ventas` (`id_venta`) ON DELETE CASCADE,
  CONSTRAINT `fk_detalle_venta_producto` FOREIGN KEY (`id_producto`) REFERENCES `productos` (`id_producto`),
  CONSTRAINT `ck_vdet_cant` CHECK ((`cantidad` > 0)),
  CONSTRAINT `ck_vdet_precio` CHECK ((`precio_unitario` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ventas_detalle`
--

LOCK TABLES `ventas_detalle` WRITE;
/*!40000 ALTER TABLE `ventas_detalle` DISABLE KEYS */;
INSERT INTO `ventas_detalle` VALUES (5,2,1,7,6.00),(6,2,3,3,24.00),(7,2,5,3,27.52),(8,3,4,43,12.61),(9,4,2,5,16.25);
/*!40000 ALTER TABLE `ventas_detalle` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'db_punto_venta'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-25  6:53:44
