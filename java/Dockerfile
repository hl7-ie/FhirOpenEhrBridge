# syntax=docker/dockerfile:1

# ---- Build stage ---------------------------------------------------------
FROM maven:3.9-eclipse-temurin-17 AS build
WORKDIR /src
COPY pom.xml ./
RUN mvn -q -B dependency:go-offline
COPY src ./src
RUN mvn -q -B package -DskipTests

# ---- Runtime stage -------------------------------------------------------
FROM eclipse-temurin:17-jre AS runtime
WORKDIR /app
COPY --from=build /src/target/fhir-openehr-bridge.jar ./app.jar
RUN useradd -r -u 10001 appuser
USER appuser
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["java", "-jar", "/app/app.jar"]
