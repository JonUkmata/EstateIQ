export type PhoneCountry = {
  code: string
  label: string
}

export const phoneCountries: PhoneCountry[] = [
  { code: '+383', label: 'Kosovo (+383)' },
  { code: '+355', label: 'Albania (+355)' },
  { code: '+389', label: 'North Macedonia (+389)' },
  { code: '+49', label: 'Germany (+49)' },
  { code: '+41', label: 'Switzerland (+41)' },
  { code: '+44', label: 'United Kingdom (+44)' },
  { code: '+1', label: 'United States (+1)' },
]

export function buildPhoneNumber(countryCode: string, localNumber: string) {
  const cleanedLocalNumber = localNumber.trim().replace(/^\+/, '')
  return `${countryCode} ${cleanedLocalNumber}`.trim()
}

export function isValidLocalPhoneNumber(localNumber: string) {
  const trimmed = localNumber.trim()
  const digitCount = trimmed.replace(/\D/g, '').length

  return digitCount >= 5 && /^[0-9\s\-()]+$/.test(trimmed)
}
