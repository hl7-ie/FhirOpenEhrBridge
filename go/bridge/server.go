package bridge

import (
	"encoding/json"
	"io"
	"net/http"
)

// NewMux builds the HTTP handler exposing the translation endpoints.
func NewMux() *http.ServeMux {
	svc := NewTranslationService()
	mux := http.NewServeMux()

	mux.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
		writeJSON(w, http.StatusOK, map[string]string{"status": "Healthy", "service": "FHIR-OpenEHR-Bridge"})
	})

	mux.HandleFunc("/api/translate/fhir-to-openehr", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			w.WriteHeader(http.StatusMethodNotAllowed)
			return
		}
		body, _ := io.ReadAll(r.Body)
		res := svc.FhirToOpenEhr(string(body))
		status := http.StatusOK
		if !res.Success {
			status = http.StatusBadRequest
		}
		writeJSON(w, status, map[string]any{"success": res.Success, "result": res.Value, "issues": res.Issues})
	})

	mux.HandleFunc("/api/translate/openehr-to-fhir", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			w.WriteHeader(http.StatusMethodNotAllowed)
			return
		}
		var comp OpenEhrComposition
		if err := json.NewDecoder(r.Body).Decode(&comp); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]any{
				"success": false, "result": nil,
				"issues": []ValidationIssue{{SeverityError, "Invalid JSON body: " + err.Error(), ""}},
			})
			return
		}
		res := svc.OpenEhrToFhir(&comp)
		if !res.Success {
			writeJSON(w, http.StatusBadRequest, map[string]any{"success": false, "result": nil, "issues": res.Issues})
			return
		}
		writeJSON(w, http.StatusOK, map[string]any{"success": true, "result": json.RawMessage(res.Value), "issues": res.Issues})
	})

	return mux
}

func writeJSON(w http.ResponseWriter, status int, body any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(body)
}
