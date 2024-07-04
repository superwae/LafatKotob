export interface MyTokenPayload {
  sub: string; //username
  jti: string; // Token ID
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string; // User's unique identifier
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string; // User's role
  exp: number; // Expiration time
  iss: string; // Issuer
  aud: string; // Audience
}
