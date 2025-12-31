// BACKUP: Contenido original con E2E_RegisterAndLoginImmediately_Success que falla cuando se ejecuta con otros tests
// El problema es que el test falla con 401 Unauthorized cuando se ejecuta en batch, pero pasa cuando se ejecuta aisladamente
// Causa probable: conflicto con datos del test anterior o problema con scopes de DbContext
