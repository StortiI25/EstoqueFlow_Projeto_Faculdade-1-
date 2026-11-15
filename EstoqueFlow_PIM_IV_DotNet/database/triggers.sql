-- Triggers demonstrativas para SQLite.
-- Aplicar após criar as tabelas do banco.
CREATE TRIGGER IF NOT EXISTS TR_StockMovements_NoUpdate
BEFORE UPDATE ON StockMovements
BEGIN
    SELECT RAISE(ABORT, 'O histórico de movimentações não pode ser alterado.');
END;

CREATE TRIGGER IF NOT EXISTS TR_StockMovements_NoDelete
BEFORE DELETE ON StockMovements
BEGIN
    SELECT RAISE(ABORT, 'O histórico de movimentações não pode ser excluído.');
END;
