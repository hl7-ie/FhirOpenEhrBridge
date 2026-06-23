//! HTTP API for the Rust port (axum).

use axum::{
    http::StatusCode,
    response::IntoResponse,
    routing::{get, post},
    Json, Router,
};
use fhir_openehr_bridge::{models::OpenEhrComposition, TranslationService};
use serde_json::{json, Value};

fn app() -> Router {
    Router::new()
        .route("/health", get(health))
        .route("/api/translate/fhir-to-openehr", post(fhir_to_openehr))
        .route("/api/translate/openehr-to-fhir", post(openehr_to_fhir))
}

async fn health() -> Json<Value> {
    Json(json!({ "status": "Healthy", "service": "FHIR-OpenEHR-Bridge" }))
}

async fn fhir_to_openehr(body: String) -> impl IntoResponse {
    let out = TranslationService::new().fhir_to_openehr(&body);
    let status = if out.success { StatusCode::OK } else { StatusCode::BAD_REQUEST };
    (status, Json(json!({ "success": out.success, "result": out.value, "issues": out.issues })))
}

async fn openehr_to_fhir(Json(comp): Json<OpenEhrComposition>) -> impl IntoResponse {
    let out = TranslationService::new().openehr_to_fhir(&comp);
    let status = if out.success { StatusCode::OK } else { StatusCode::BAD_REQUEST };
    (status, Json(json!({ "success": out.success, "result": out.value, "issues": out.issues })))
}

#[tokio::main]
async fn main() {
    let port = std::env::var("PORT").unwrap_or_else(|_| "8080".to_string());
    let addr = format!("0.0.0.0:{port}");
    let listener = tokio::net::TcpListener::bind(&addr).await.expect("bind");
    println!("FHIR-OpenEHR-Bridge (Rust) listening on :{port}");
    axum::serve(listener, app()).await.expect("serve");
}
