-- Database-per-service: um database e um login por serviço.
-- Cada login só enxerga o próprio database, então o isolamento é garantido pelo SQL Server,
-- não só por convenção. Script idempotente: pode rodar várias vezes.
-- As senhas chegam como variáveis do sqlcmd (-v), vindas do .env.

-------------------------------------------------------------------------------
IF DB_ID('TransferenciasDb') IS NULL CREATE DATABASE TransferenciasDb;
GO
IF SUSER_ID('transferencias_app') IS NULL
    CREATE LOGIN transferencias_app WITH PASSWORD = '$(TransferenciasPwd)', DEFAULT_DATABASE = TransferenciasDb;
GO
USE TransferenciasDb;
IF USER_ID('transferencias_app') IS NULL CREATE USER transferencias_app FOR LOGIN transferencias_app;
ALTER ROLE db_owner ADD MEMBER transferencias_app;
GO

-------------------------------------------------------------------------------
USE master;
IF DB_ID('LimitesDb') IS NULL CREATE DATABASE LimitesDb;
GO
IF SUSER_ID('limites_app') IS NULL
    CREATE LOGIN limites_app WITH PASSWORD = '$(LimitesPwd)', DEFAULT_DATABASE = LimitesDb;
GO
USE LimitesDb;
IF USER_ID('limites_app') IS NULL CREATE USER limites_app FOR LOGIN limites_app;
ALTER ROLE db_owner ADD MEMBER limites_app;
GO

-------------------------------------------------------------------------------
USE master;
IF DB_ID('ContasDb') IS NULL CREATE DATABASE ContasDb;
GO
IF SUSER_ID('contas_app') IS NULL
    CREATE LOGIN contas_app WITH PASSWORD = '$(ContasPwd)', DEFAULT_DATABASE = ContasDb;
GO
USE ContasDb;
IF USER_ID('contas_app') IS NULL CREATE USER contas_app FOR LOGIN contas_app;
ALTER ROLE db_owner ADD MEMBER contas_app;
GO

-------------------------------------------------------------------------------
USE master;
IF DB_ID('NotificacoesDb') IS NULL CREATE DATABASE NotificacoesDb;
GO
IF SUSER_ID('notificacoes_app') IS NULL
    CREATE LOGIN notificacoes_app WITH PASSWORD = '$(NotificacoesPwd)', DEFAULT_DATABASE = NotificacoesDb;
GO
USE NotificacoesDb;
IF USER_ID('notificacoes_app') IS NULL CREATE USER notificacoes_app FOR LOGIN notificacoes_app;
ALTER ROLE db_owner ADD MEMBER notificacoes_app;
GO

PRINT 'Databases e logins prontos.';
