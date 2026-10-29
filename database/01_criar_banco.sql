/* =============================================================
   APRENDER MAIS - Script 01: criação do banco de dados
   SGBD: Microsoft SQL Server 2019+ (ou Azure SQL Managed Instance)

   Azure SQL Database: crie o banco "AprenderMais" pelo portal, conecte
   direto nele e rode os scripts 02 a 06 sem as linhas "USE AprenderMais".
   ============================================================= */

IF DB_ID('AprenderMais') IS NULL
BEGIN
    CREATE DATABASE AprenderMais;
END
GO

USE AprenderMais;
GO
