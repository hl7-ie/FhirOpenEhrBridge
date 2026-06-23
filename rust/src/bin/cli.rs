//! Tiny CLI demo for the Rust port.
//!
//!   cli fhir-to-openehr <patient.json>
//!   cli openehr-to-fhir <composition.json>

use fhir_openehr_bridge::{models::OpenEhrComposition, TranslationService};
use serde_json::json;
use std::process::exit;

fn main() {
    let args: Vec<String> = std::env::args().skip(1).collect();
    if args.len() < 2 {
        eprintln!("usage: cli <fhir-to-openehr|openehr-to-fhir> <file.json>");
        exit(2);
    }
    let (direction, file) = (&args[0], &args[1]);
    let content = match std::fs::read_to_string(file) {
        Ok(c) => c,
        Err(e) => {
            eprintln!("{e}");
            exit(2);
        }
    };
    let svc = TranslationService::new();

    let (ok, payload) = match direction.as_str() {
        "fhir-to-openehr" => {
            let out = svc.fhir_to_openehr(&content);
            (out.success, json!({ "success": out.success, "result": out.value, "issues": out.issues }))
        }
        "openehr-to-fhir" => {
            let comp: OpenEhrComposition = match serde_json::from_str(&content) {
                Ok(c) => c,
                Err(e) => {
                    eprintln!("{e}");
                    exit(2);
                }
            };
            let out = svc.openehr_to_fhir(&comp);
            (out.success, json!({ "success": out.success, "result": out.value, "issues": out.issues }))
        }
        other => {
            eprintln!("unknown direction: {other}");
            exit(2);
        }
    };

    println!("{}", serde_json::to_string_pretty(&payload).unwrap());
    exit(if ok { 0 } else { 1 });
}
