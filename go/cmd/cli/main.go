// Command cli translates a sample file in either direction.
//
//	cli fhir-to-openehr <patient.json>
//	cli openehr-to-fhir <composition.json>
package main

import (
	"encoding/json"
	"fmt"
	"os"

	"github.com/hl7-ie/FhirOpenEhrBridge/go/bridge"
)

func main() {
	os.Exit(run(os.Args[1:]))
}

func run(args []string) int {
	if len(args) < 2 {
		fmt.Fprintln(os.Stderr, "usage: cli <fhir-to-openehr|openehr-to-fhir> <file.json>")
		return 2
	}
	direction, file := args[0], args[1]
	data, err := os.ReadFile(file)
	if err != nil {
		fmt.Fprintln(os.Stderr, err)
		return 2
	}
	svc := bridge.NewTranslationService()

	switch direction {
	case "fhir-to-openehr":
		res := svc.FhirToOpenEhr(string(data))
		printJSON(map[string]any{"success": res.Success, "result": res.Value, "issues": res.Issues})
		return boolToCode(res.Success)
	case "openehr-to-fhir":
		var comp bridge.OpenEhrComposition
		if err := json.Unmarshal(data, &comp); err != nil {
			fmt.Fprintln(os.Stderr, err)
			return 2
		}
		res := svc.OpenEhrToFhir(&comp)
		var result any
		if res.Value != "" {
			_ = json.Unmarshal([]byte(res.Value), &result)
		}
		printJSON(map[string]any{"success": res.Success, "result": result, "issues": res.Issues})
		return boolToCode(res.Success)
	default:
		fmt.Fprintf(os.Stderr, "unknown direction: %s\n", direction)
		return 2
	}
}

func printJSON(v any) {
	out, _ := json.MarshalIndent(v, "", "  ")
	fmt.Println(string(out))
}

func boolToCode(ok bool) int {
	if ok {
		return 0
	}
	return 1
}
