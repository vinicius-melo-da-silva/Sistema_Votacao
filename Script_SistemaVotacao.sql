/*SISTEMA DE VOTAÇÃO*/

/*
Número: 14

Aluno: VINICIUS MELO DA SILVA
Tema: Sistema de Votação Online
Tabela 1: Eleitores (id_eleitor, nome, título_eleitoral)
Tabela 2: Candidatos (id_candidato, nome, cpf)
Tabela 3: Votos (id_voto, id_eleitor, id_candidato)
Relação: Eleitores ↔ Votos ↔ Candidatos

Crie o respectivo banco de dados. Se for preciso, faça os ajustes necessários (inclusão de tabelas, ajuste nos relacionamentos)
Faça o login, cadastro, listagem, editar, excluir de todas as tabelas
Acrescente em uma das tabelas o cadastro de uma foto/imagem
Controle de acesso: As consultas (listagem) são visiveis para qualquer usuário sem login. As outras ações serão destinadas apenas para quem tem login.
Serão 3 tipos de usuário: 
Admin pode efetuar todas as operações
gerente pode fazer todas as operações (menos cadastrar novos usuários)
comum pode apenas cadastrar e consultar.

*/

-- Criando banco
CREATE DATABASE sistemavoto_vinicius;

-- Criando banco
USE sistemavoto_vinicius;

-- Tabela de Eleitores
CREATE TABLE Eleitores (
    id_eleitor INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    titulo_eleitoral VARCHAR(20) UNIQUE NOT NULL,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP);

-- Tabela de Candidatos
CREATE TABLE Candidatos (
    id_candidato INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    cpf VARCHAR(14) UNIQUE NOT NULL,
    titulo_eleitoral VARCHAR(20) UNIQUE NOT NULL,
    foto VARCHAR(255),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP);

-- Tabela de Usuarios
CREATE TABLE Usuarios (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(100) NOT NULL,
    titulo_eleitoral VARCHAR(20) UNIQUE NOT NULL,
    senha_hash VARCHAR(255) NOT NULL,
    role ENUM('Gerente', 'Adm', 'Comum'),
    ativo TINYINT(1) DEFAULT 1,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (titulo_eleitoral) REFERENCES Eleitores(titulo_eleitoral));

-- Tabela de Votos
CREATE TABLE Votos (
    id_voto INT AUTO_INCREMENT PRIMARY KEY,
    id_eleitor INT NOT NULL UNIQUE KEY,
    id_candidato INT NOT NULL,
    data_voto DATETIME DEFAULT CURRENT_TIMESTAMP,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_eleitor) REFERENCES Eleitores(id_eleitor),
    FOREIGN KEY (id_candidato) REFERENCES Candidatos(id_candidato));

-- Inserts de Eleitores
INSERT INTO Eleitores (nome, titulo_eleitoral) VALUES 
('João Silva', '123456789012'),
('Maria Santos', '987654321098'),
('Pedro Oliveira', '456789123456'),
('Ana Costa', '321654987321'),
('Carlos Souza', '789123456789');




/*PROCEDURES*/


/*ELEITORES*/

-- Insert - Criar Eleitor
DELIMITER //
CREATE PROCEDURE CriarEleitor(
    IN p_nome VARCHAR(100),
    IN p_titulo_eleitoral VARCHAR(20)
)
BEGIN
    DECLARE titulo_count INT;
    SELECT COUNT(*) INTO titulo_count 
    FROM Eleitores 
    WHERE titulo_eleitoral = p_titulo_eleitoral;
    IF titulo_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Título eleitoral já cadastrado.';
    ELSE
        INSERT INTO Eleitores (nome, titulo_eleitoral)
        VALUES (p_nome, p_titulo_eleitoral);
        SELECT 'Eleitor criado com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;

-- Select - Listar Eleitor
DELIMITER //
CREATE PROCEDURE ListarEleitores(
    IN p_id_eleitor INT
)
BEGIN
    IF p_id_eleitor IS NULL THEN
        SELECT 
            id_eleitor,
            nome,
            titulo_eleitoral,
            criado_em
        FROM Eleitores
        ORDER BY nome;
    ELSE
        SELECT 
            id_eleitor,
            nome,
            titulo_eleitoral,
            criado_em
        FROM Eleitores
        WHERE id_eleitor = p_id_eleitor;
    END IF;
END //
DELIMITER ;

-- Update - Editar Eleitor
DELIMITER //
CREATE PROCEDURE EditarEleitor(
    IN p_id_eleitor INT,
    IN p_nome VARCHAR(100),
    IN p_titulo_eleitoral VARCHAR(20)
)
BEGIN
    DECLARE eleitor_count INT;
    DECLARE titulo_count INT;
    SELECT COUNT(*) INTO eleitor_count 
    FROM Eleitores 
    WHERE id_eleitor = p_id_eleitor;
    IF eleitor_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Eleitor não encontrado.';
    END IF;
    SELECT COUNT(*) INTO titulo_count 
    FROM Eleitores 
    WHERE titulo_eleitoral = p_titulo_eleitoral AND id_eleitor != p_id_eleitor;
    IF titulo_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Título eleitoral já cadastrado para outro eleitor.';
    ELSE
        UPDATE Eleitores 
        SET nome = p_nome, titulo_eleitoral = p_titulo_eleitoral
        WHERE id_eleitor = p_id_eleitor;
        SELECT 'Eleitor atualizado com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;

DELIMITER //

-- Delete - Editar Eleitor
CREATE PROCEDURE ExcluirEleitor(
    IN p_id_eleitor INT
)
BEGIN
    DECLARE eleitor_count INT;
    DECLARE voto_count INT;
    DECLARE usuario_count INT;
    SELECT COUNT(*) INTO eleitor_count 
    FROM Eleitores 
    WHERE id_eleitor = p_id_eleitor;
    IF eleitor_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Eleitor não encontrado.';
    END IF;
    SELECT COUNT(*) INTO voto_count 
    FROM Votos 
    WHERE id_eleitor = p_id_eleitor;
    IF voto_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Não é possível excluir eleitor com voto registrado.';
    END IF;
    SELECT COUNT(*) INTO usuario_count 
    FROM Usuarios u
    INNER JOIN Eleitores e ON u.titulo_eleitoral = e.titulo_eleitoral
    WHERE e.id_eleitor = p_id_eleitor;
    IF usuario_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Não é possível excluir eleitor vinculado a um usuário.';
    ELSE
        DELETE FROM Eleitores WHERE id_eleitor = p_id_eleitor;
        SELECT 'Eleitor excluído com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;

/*Candidatos*/

-- Insert - Criar Candidatos
DELIMITER //
CREATE PROCEDURE CriarCandidato(
    IN p_nome VARCHAR(100),
    IN p_cpf VARCHAR(14),
    IN p_titulo_eleitoral VARCHAR(20),
    IN p_foto VARCHAR(255)
)
BEGIN
    DECLARE cpf_count INT;
    DECLARE titulo_count INT;
    SELECT COUNT(*) INTO cpf_count 
    FROM Candidatos 
    WHERE cpf = p_cpf;
    IF cpf_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'CPF já cadastrado.';
    END IF;
    SELECT COUNT(*) INTO titulo_count 
    FROM Candidatos 
    WHERE titulo_eleitoral = p_titulo_eleitoral;
    IF titulo_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Título eleitoral já cadastrado.';
    ELSE
        INSERT INTO Candidatos (nome, cpf, titulo_eleitoral, foto)
        VALUES (p_nome, p_cpf, p_titulo_eleitoral, p_foto);
        SELECT 'Candidato criado com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;

-- Select - Listar Candidatos
DELIMITER //
CREATE PROCEDURE ListarCandidatos(
    IN p_id_candidato INT)
BEGIN
    IF p_id_candidato IS NULL THEN
        SELECT 
            id_candidato,
            nome,
            cpf,
            titulo_eleitoral,
            foto,
            criado_em
        FROM Candidatos
        ORDER BY nome;
    ELSE
        SELECT 
            id_candidato,
            nome,
            cpf,
            titulo_eleitoral,
            foto,
            criado_em
        FROM Candidatos
        WHERE id_candidato = p_id_candidato;
    END IF;
END //
DELIMITER ;

-- Update - Editar Candidatos
DELIMITER //
CREATE PROCEDURE EditarCandidato(
    IN p_id_candidato INT,
    IN p_nome VARCHAR(100),
    IN p_cpf VARCHAR(14),
    IN p_titulo_eleitoral VARCHAR(20),
    IN p_foto VARCHAR(255)
)
BEGIN
    DECLARE candidato_count INT;
    DECLARE cpf_count INT;
    DECLARE titulo_count INT;
    -- Verificar se candidato existe
    SELECT COUNT(*) INTO candidato_count 
    FROM Candidatos 
    WHERE id_candidato = p_id_candidato;
    IF candidato_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Candidato não encontrado.';
    END IF;
    SELECT COUNT(*) INTO cpf_count 
    FROM Candidatos 
    WHERE cpf = p_cpf AND id_candidato != p_id_candidato;
    IF cpf_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'CPF já cadastrado para outro candidato.';
    END IF;
    SELECT COUNT(*) INTO titulo_count 
    FROM Candidatos 
    WHERE titulo_eleitoral = p_titulo_eleitoral AND id_candidato != p_id_candidato;
    IF titulo_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Título eleitoral já cadastrado para outro candidato.';
    ELSE
        UPDATE Candidatos 
        SET nome = p_nome, 
            cpf = p_cpf, 
            titulo_eleitoral = p_titulo_eleitoral, 
            foto = p_foto
        WHERE id_candidato = p_id_candidato;
        SELECT 'Candidato atualizado com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;

-- Delete - Deletar Candidatos
DELIMITER //
CREATE PROCEDURE ExcluirCandidato(
    IN p_id_candidato INT
)
BEGIN
    DECLARE candidato_count INT;
    DECLARE voto_count INT;
    SELECT COUNT(*) INTO candidato_count 
    FROM Candidatos 
    WHERE id_candidato = p_id_candidato;
    IF candidato_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Candidato não encontrado.';
    END IF;
    SELECT COUNT(*) INTO voto_count 
    FROM Votos 
    WHERE id_candidato = p_id_candidato;
    IF voto_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Não é possível excluir candidato com voto registrado.';
    ELSE
        DELETE FROM Candidatos WHERE id_candidato = p_id_candidato;
        SELECT 'Candidato excluído com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;



/*Usuarios*/

-- Insert - Criar Usuarios
DELIMITER //
CREATE PROCEDURE CriarUsuario(
    IN p_nome VARCHAR(100),
    IN p_titulo_eleitoral VARCHAR(20),
    IN p_senha_hash VARCHAR(255),
    IN p_role ENUM('Gerente', 'Adm', 'Comum')
)
BEGIN
    DECLARE titulo_count INT;
    DECLARE eleitor_count INT;
    SELECT COUNT(*) INTO eleitor_count 
    FROM Eleitores 
    WHERE titulo_eleitoral = p_titulo_eleitoral;
    IF eleitor_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Título eleitoral não encontrado na tabela de Eleitores.';
    END IF;
    SELECT COUNT(*) INTO titulo_count 
    FROM Usuarios 
    WHERE titulo_eleitoral = p_titulo_eleitoral;
    IF titulo_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Título eleitoral já cadastrado como usuário.';
    ELSE
        INSERT INTO Usuarios (nome, titulo_eleitoral, senha_hash, role)
        VALUES (p_nome, p_titulo_eleitoral, p_senha_hash, p_role);
        SELECT 'Usuário criado com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;

-- Select - Listar Usuarios
DELIMITER //
CREATE PROCEDURE ListarUsuarios(
    IN p_id INT,
    IN p_ativos_apenas BOOLEAN
)
BEGIN
    IF p_id IS NULL THEN
        IF p_ativos_apenas THEN
            SELECT 
                u.id,
                u.nome,
                u.titulo_eleitoral,
                u.role,
                u.ativo,
                u.criado_em,
                e.id_eleitor
            FROM Usuarios u
            INNER JOIN Eleitores e ON u.titulo_eleitoral = e.titulo_eleitoral
            WHERE u.ativo = 1
            ORDER BY u.nome;
        ELSE
            SELECT 
                u.id,
                u.nome,
                u.titulo_eleitoral,
                u.role,
                u.ativo,
                u.criado_em,
                e.id_eleitor
            FROM Usuarios u
            INNER JOIN Eleitores e ON u.titulo_eleitoral = e.titulo_eleitoral
            ORDER BY u.nome;
        END IF;
    ELSE
        SELECT 
            u.id,
            u.nome,
            u.titulo_eleitoral,
            u.role,
            u.ativo,
            u.criado_em,
            e.id_eleitor
        FROM Usuarios u
        INNER JOIN Eleitores e ON u.titulo_eleitoral = e.titulo_eleitoral
        WHERE u.id = p_id;
    END IF;
END //
DELIMITER ;

-- Update - Editar Usuarios
DELIMITER //
CREATE PROCEDURE EditarUsuario(
    IN p_id INT,
    IN p_nome VARCHAR(100),
    IN p_senha_hash VARCHAR(255),
    IN p_role ENUM('Gerente', 'Adm', 'Comum'),
    IN p_ativo TINYINT(1)
)
BEGIN
    DECLARE usuario_count INT;
    SELECT COUNT(*) INTO usuario_count 
    FROM Usuarios 
    WHERE id = p_id;
    IF usuario_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Usuário não encontrado.';
    ELSE
        UPDATE Usuarios 
        SET nome = p_nome,
            senha_hash = p_senha_hash,
            role = p_role,
            ativo = p_ativo
        WHERE id = p_id;
        SELECT 'Usuário atualizado com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;

-- Delete - Deletar Usuarios
DELIMITER //
CREATE PROCEDURE ExcluirUsuario(
    IN p_id INT
)
BEGIN
    DECLARE usuario_count INT;
    SELECT COUNT(*) INTO usuario_count 
    FROM Usuarios 
    WHERE id = p_id;
    IF usuario_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Usuário não encontrado.';
    ELSE
        DELETE FROM Usuarios WHERE id = p_id;
        SELECT 'Usuário excluído com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;



/*Votos*/

-- Insert - Criar Votos
DELIMITER //
CREATE PROCEDURE RegistrarVoto(
    IN p_id_eleitor INT,
    IN p_id_candidato INT
)
BEGIN
    DECLARE eleitor_count INT;
    DECLARE candidato_count INT;
    DECLARE voto_count INT;
    DECLARE eleitor_ativo INT;
    SELECT COUNT(*) INTO eleitor_count 
    FROM Eleitores 
    WHERE id_eleitor = p_id_eleitor;
    IF eleitor_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Eleitor não encontrado.';
    END IF;
    SELECT COUNT(*) INTO candidato_count 
    FROM Candidatos 
    WHERE id_candidato = p_id_candidato;
    IF candidato_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Candidato não encontrado.';
    END IF;
    SELECT COUNT(*) INTO voto_count 
    FROM Votos 
    WHERE id_eleitor = p_id_eleitor;
    IF voto_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Eleitor já realizou o voto.';
    END IF;
    INSERT INTO Votos (id_eleitor, id_candidato)
    VALUES (p_id_eleitor, p_id_candidato);
    SELECT 'Voto registrado com sucesso.' AS mensagem;
END //
DELIMITER ;

-- Select - Listar Votos
DELIMITER //
CREATE PROCEDURE ListarVotos(
    IN p_id_voto INT,
    IN p_id_eleitor INT,
    IN p_id_candidato INT
)
BEGIN
    IF p_id_voto IS NOT NULL THEN
        SELECT 
            v.id_voto,
            v.id_eleitor,
            e.nome AS nome_eleitor,
            e.titulo_eleitoral AS titulo_eleitor,
            v.id_candidato,
            c.nome AS nome_candidato,
            v.data_voto,
            v.criado_em
        FROM Votos v
        INNER JOIN Eleitores e ON v.id_eleitor = e.id_eleitor
        INNER JOIN Candidatos c ON v.id_candidato = c.id_candidato
        WHERE v.id_voto = p_id_voto;
    ELSEIF p_id_eleitor IS NOT NULL THEN
        SELECT 
            v.id_voto,
            v.id_eleitor,
            e.nome AS nome_eleitor,
            e.titulo_eleitoral AS titulo_eleitor,
            v.id_candidato,
            c.nome AS nome_candidato,
            v.data_voto,
            v.criado_em
        FROM Votos v
        INNER JOIN Eleitores e ON v.id_eleitor = e.id_eleitor
        INNER JOIN Candidatos c ON v.id_candidato = c.id_candidato
        WHERE v.id_eleitor = p_id_eleitor;
    ELSEIF p_id_candidato IS NOT NULL THEN
        SELECT 
            v.id_voto,
            v.id_eleitor,
            e.nome AS nome_eleitor,
            e.titulo_eleitoral AS titulo_eleitor,
            v.id_candidato,
            c.nome AS nome_candidato,
            v.data_voto,
            v.criado_em
        FROM Votos v
        INNER JOIN Eleitores e ON v.id_eleitor = e.id_eleitor
        INNER JOIN Candidatos c ON v.id_candidato = c.id_candidato
        WHERE v.id_candidato = p_id_candidato
        ORDER BY v.data_voto;
    ELSE
        SELECT 
            v.id_voto,
            v.id_eleitor,
            e.nome AS nome_eleitor,
            e.titulo_eleitoral AS titulo_eleitor,
            v.id_candidato,
            c.nome AS nome_candidato,
            v.data_voto,
            v.criado_em
        FROM Votos v
        INNER JOIN Eleitores e ON v.id_eleitor = e.id_eleitor
        INNER JOIN Candidatos c ON v.id_candidato = c.id_candidato
        ORDER BY v.data_voto DESC;
    END IF;
END //
DELIMITER ;

-- Update - Editar Votos
DELIMITER //
CREATE PROCEDURE EditarVoto(
    IN p_id_voto INT,
    IN p_id_candidato_novo INT
)
BEGIN
    DECLARE voto_count INT;
    DECLARE candidato_count INT;
    DECLARE id_eleitor_existente INT;
    DECLARE id_candidato_existente INT;
    SELECT COUNT(*), id_eleitor, id_candidato 
    INTO voto_count, id_eleitor_existente, id_candidato_existente
    FROM Votos 
    WHERE id_voto = p_id_voto;
    IF voto_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Voto não encontrado.';
    END IF;
    SELECT COUNT(*) INTO candidato_count 
    FROM Candidatos 
    WHERE id_candidato = p_id_candidato_novo;
    IF candidato_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Novo candidato não encontrado.';
    END IF;
    IF id_candidato_existente = p_id_candidato_novo THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'O novo candidato é o mesmo do voto atual.';
    END IF;
    UPDATE Votos 
    SET id_candidato = p_id_candidato_novo,
        data_voto = CURRENT_TIMESTAMP
    WHERE id_voto = p_id_voto;
    SELECT 
        'Voto atualizado com sucesso.' AS mensagem,
        v.id_voto,
        v.id_eleitor,
        e.nome AS nome_eleitor,
        id_candidato_existente AS id_candidato_anterior,
        p_id_candidato_novo AS id_candidato_novo,
        v.data_voto
    FROM Votos v
    INNER JOIN Eleitores e ON v.id_eleitor = e.id_eleitor
    WHERE v.id_voto = p_id_voto;
END //
DELIMITER ;


-- Delete - Deletar Votos
DELIMITER //
CREATE PROCEDURE ExcluirVoto(
    IN p_id_voto INT
)
BEGIN
    DECLARE voto_count INT;
    SELECT COUNT(*) INTO voto_count 
    FROM Votos 
    WHERE id_voto = p_id_voto;
    IF voto_count = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Voto não encontrado.';
    ELSE
        DELETE FROM Votos WHERE id_voto = p_id_voto;
        SELECT 'Voto excluído com sucesso.' AS mensagem;
    END IF;
END //
DELIMITER ;

-- RESULTADO DA ELEIÇÃO
DELIMITER //
CREATE PROCEDURE ObterResultadoEleicao()
BEGIN
    SELECT 
        c.id_candidato,
        c.nome AS nome_candidato,
        c.foto,
        COUNT(v.id_voto) AS total_votos,
        ROUND((COUNT(v.id_voto) * 100.0 / (SELECT COUNT(*) FROM Votos)), 2) AS percentual
    FROM Candidatos c
    LEFT JOIN Votos v ON c.id_candidato = v.id_candidato
    GROUP BY c.id_candidato, c.nome, c.foto
    ORDER BY total_votos DESC;
END //
DELIMITER ;