// Command api runs the HTTP translation service.
package main

import (
	"log"
	"net/http"
	"os"

	"github.com/hl7-ie/FhirOpenEhrBridge/go/bridge"
)

func main() {
	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}
	log.Printf("FHIR-OpenEHR-Bridge (Go) listening on :%s", port)
	if err := http.ListenAndServe(":"+port, bridge.NewMux()); err != nil {
		log.Fatal(err)
	}
}
