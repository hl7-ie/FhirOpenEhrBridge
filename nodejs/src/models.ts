// Canonical, language-neutral models for the FHIR <-> openEHR demographics
// translation. Mirrors the .NET reference implementation (see docs/POLYGLOT.md).

export interface OpenEhrIdentifier {
  id?: string;
  issuer?: string;
  type?: string;
}

export interface OpenEhrAddress {
  line?: string;
  city?: string;
  postalCode?: string;
  country?: string;
}

export interface OpenEhrDemographics {
  familyName?: string;
  givenName?: string;
  gender?: string;
  birthDate?: string;
  identifiers: OpenEhrIdentifier[];
  address?: OpenEhrAddress | null;
}

export interface OpenEhrEhrStatus {
  subjectId?: string;
  subjectNamespace: string;
  isQueryable: boolean;
  isModifiable: boolean;
}

export interface OpenEhrComposition {
  archetypeNodeId: string;
  name: string;
  language: string;
  territory: string;
  category: string;
  startTime?: string;
  ehrStatus: OpenEhrEhrStatus;
  demographics: OpenEhrDemographics;
}

export type ValidationSeverity = 'error' | 'warning' | 'information';

export interface ValidationIssue {
  severity: ValidationSeverity;
  message: string;
  location?: string;
}

export interface TranslationResult<T> {
  success: boolean;
  value?: T;
  issues: ValidationIssue[];
}

/** Minimal FHIR Patient shape — only the subset the demographics mapping uses. */
export interface FhirPatient {
  resourceType?: string;
  id?: string;
  identifier?: Array<{
    system?: string;
    value?: string;
    type?: { text?: string; coding?: Array<{ code?: string }> };
  }>;
  name?: Array<{ family?: string; given?: string[]; use?: string }>;
  gender?: string;
  birthDate?: string;
  address?: Array<{ line?: string[]; city?: string; postalCode?: string; country?: string }>;
}
