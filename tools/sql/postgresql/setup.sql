CREATE GROUP testgroup;
CREATE USER testeditor WITH CREATEDB IN GROUP testgroup
       ENCRYPTED PASSWORD 'sekrit';
CREATE USER testuser  WITH NOCREATEDB IN GROUP testgroup
       ENCRYPTED PASSWORD 'password';
CREATE DATABASE sample WITH OWNER = testeditor;

GRANT CONNECT ON DATABASE sample TO testuser;

GRANT USAGE ON SCHEMA public TO testuser;
REVOKE CREATE ON SCHEMA public FROM testuser;

