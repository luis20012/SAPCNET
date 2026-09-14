-- Datos de prueba: 10 provincias argentinas para la base local (ies9021_scs).
-- Idempotente: se puede correr varias veces sin duplicar filas.
-- id_country = 2 -> Argentina en la tabla country.
-- id_user_create = '0' -> carga del sistema (la tabla user está vacía en local).
-- Correr con: mysql --default-character-set=utf8mb4 -u ies9021_userdb -p ies9021_scs < db/seed_provincias.sql

INSERT INTO province (id_province, id_country, name, id_user_create, date_create) VALUES
    (1,  '2', 'Buenos Aires', '0', NOW()),
    (2,  '2', 'Mendoza',      '0', NOW()),
    (3,  '2', 'Córdoba',      '0', NOW()),
    (4,  '2', 'Santa Fe',     '0', NOW()),
    (5,  '2', 'Entre Ríos',   '0', NOW()),
    (6,  '2', 'Tucumán',      '0', NOW()),
    (7,  '2', 'Salta',        '0', NOW()),
    (8,  '2', 'Neuquén',      '0', NOW()),
    (9,  '2', 'Río Negro',    '0', NOW()),
    (10, '2', 'Chubut',       '0', NOW())
ON DUPLICATE KEY UPDATE name = VALUES(name);
