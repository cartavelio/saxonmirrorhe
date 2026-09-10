FROM postgres:13.2-alpine as postgres-tests

COPY --from=bootstrap ./postgresql/setup.sql /docker-entrypoint-initdb.d/
