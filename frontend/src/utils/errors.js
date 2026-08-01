// Normalizes error messages from the mock API (err.message) and the future
// ASP.NET backend (ProblemDetails: err.response.data.title / .message).
export function apiErrorMessage(err, fallback = 'Something went wrong.') {
  return (
    err?.response?.data?.message ||
    err?.response?.data?.title ||
    err?.message ||
    fallback
  )
}
