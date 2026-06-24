package bridge

// TranslationService is the façade selecting a mapper per direction. Mirrors the
// .NET ITranslationService; the demographics PoC has one mapper each way.
type TranslationService struct{}

// NewTranslationService constructs a TranslationService.
func NewTranslationService() *TranslationService { return &TranslationService{} }

// FhirToOpenEhr translates FHIR Patient JSON into an openEHR composition.
func (s *TranslationService) FhirToOpenEhr(fhirJSON string) Result[*OpenEhrComposition] {
	return FhirPatientToComposition(fhirJSON)
}

// OpenEhrToFhir translates an openEHR composition into a FHIR Bundle JSON string.
func (s *TranslationService) OpenEhrToFhir(c *OpenEhrComposition) Result[string] {
	return CompositionToFhirBundle(c)
}
