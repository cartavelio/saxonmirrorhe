CREATE USER testeditor IDENTIFIED BY 'sekrit';
GRANT ALL PRIVILEGES ON *.* TO testeditor;

CREATE USER testuser IDENTIFIED BY 'password';
CREATE DATABASE sample;

SELECT 'MySql setup finished' as ' ';
