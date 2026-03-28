# Accessible Forms Reference

> Official reference: [WAI Forms Tutorial](https://www.w3.org/WAI/tutorials/forms/)

## Core Rules

1. Every input must have an associated `<label>`.
2. Required fields must use `aria-required="true"` (and HTML `required`).
3. Error messages must be linked via `aria-describedby` and use `role="alert"`.
4. Invalid fields must set `aria-invalid="true"`.
5. Form groups (radio buttons, checkboxes) must use `<fieldset>` + `<legend>`.
6. Help text linked via `aria-describedby`.

## Text Input

```tsx
export interface FormFieldProps {
  id: string;
  label: string;
  type?: string;
  required?: boolean;
  error?: string;
  helpText?: string;
  value: string;
  onChange: (value: string) => void;
}

export const FormField = ({
  id,
  label,
  type = 'text',
  required = false,
  error,
  helpText,
  value,
  onChange,
}: FormFieldProps) => {
  const errorId = `${id}-error`;
  const helpId = `${id}-help`;

  // Build describedby from available descriptions
  const describedBy = [
    error ? errorId : null,
    helpText ? helpId : null,
  ].filter(Boolean).join(' ') || undefined;

  return (
    <div className="space-y-1">
      <label htmlFor={id} className="block text-sm font-medium text-gray-700">
        {label}
        {required && <span aria-hidden="true" className="text-red-500 ml-0.5">*</span>}
      </label>
      <input
        id={id}
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        required={required}
        aria-required={required}
        aria-invalid={!!error}
        aria-describedby={describedBy}
        className={cn(
          'block w-full rounded-md border px-3 py-2',
          'focus:outline-none focus:ring-2 focus:ring-offset-1',
          error
            ? 'border-red-500 focus:ring-red-500'
            : 'border-gray-300 focus:ring-blue-500'
        )}
      />
      {helpText && !error && (
        <p id={helpId} className="text-xs text-gray-500">
          {helpText}
        </p>
      )}
      {error && (
        <p id={errorId} role="alert" className="text-sm text-red-600">
          {error}
        </p>
      )}
    </div>
  );
};
```

## Select

```tsx
export const SelectField = ({
  id,
  label,
  options,
  required,
  error,
  value,
  onChange,
}: SelectFieldProps) => (
  <div className="space-y-1">
    <label htmlFor={id} className="block text-sm font-medium text-gray-700">
      {label}
      {required && <span aria-hidden="true" className="text-red-500 ml-0.5">*</span>}
    </label>
    <select
      id={id}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      required={required}
      aria-required={required}
      aria-invalid={!!error}
      className="block w-full rounded-md border border-gray-300 px-3 py-2
                 focus:outline-none focus:ring-2 focus:ring-blue-500"
    >
      <option value="">Select an option</option>
      {options.map(opt => (
        <option key={opt.value} value={opt.value}>{opt.label}</option>
      ))}
    </select>
    {error && (
      <p role="alert" className="text-sm text-red-600">{error}</p>
    )}
  </div>
);
```

## Fieldset & Legend (Radio Group / Checkbox Group)

```tsx
<fieldset>
  <legend className="text-sm font-medium text-gray-700 mb-2">
    Notification preference
  </legend>
  {options.map(option => (
    <div key={option.value} className="flex items-center gap-2">
      <input
        type="radio"
        id={`notification-${option.value}`}
        name="notification"
        value={option.value}
        checked={selected === option.value}
        onChange={() => setSelected(option.value)}
        className="h-4 w-4 text-blue-600 focus:ring-2 focus:ring-blue-500"
      />
      <label htmlFor={`notification-${option.value}`} className="text-sm">
        {option.label}
      </label>
    </div>
  ))}
</fieldset>
```

## Form-Level Error Summary

Display at the top of the form when submission fails:

```tsx
export const FormErrorSummary = ({ errors }: { errors: Record<string, string> }) => {
  const errorEntries = Object.entries(errors);
  if (errorEntries.length === 0) return null;

  return (
    <div
      role="alert"
      aria-labelledby="error-summary-title"
      className="rounded-md border border-red-200 bg-red-50 p-4 mb-4"
    >
      <h2 id="error-summary-title" className="text-sm font-semibold text-red-800">
        Please fix the following errors:
      </h2>
      <ul className="mt-2 list-disc pl-5 text-sm text-red-700">
        {errorEntries.map(([field, message]) => (
          <li key={field}>
            <a href={`#${field}`} className="underline hover:text-red-900">
              {message}
            </a>
          </li>
        ))}
      </ul>
    </div>
  );
};
```

## Loading State for Forms

```tsx
<form onSubmit={handleSubmit} aria-busy={isSubmitting}>
  {/* ...fields... */}
  <button
    type="submit"
    disabled={isSubmitting}
    aria-disabled={isSubmitting}
    className="px-4 py-2 rounded bg-blue-600 text-white disabled:opacity-50"
  >
    {isSubmitting ? (
      <>
        <span className="sr-only">Submitting...</span>
        <Spinner aria-hidden="true" className="inline h-4 w-4 mr-2 animate-spin" />
        Submitting...
      </>
    ) : (
      'Submit'
    )}
  </button>
</form>
```

## Testing Forms for Accessibility

```tsx
it('shows error when submitting empty required field', async () => {
  const user = userEvent.setup();
  render(<ContactForm onSubmit={vi.fn()} />);

  await user.click(screen.getByRole('button', { name: /submit/i }));

  expect(screen.getByRole('alert')).toHaveTextContent(/name is required/i);
  expect(screen.getByLabelText(/name/i)).toHaveAttribute('aria-invalid', 'true');
});

it('links error message to input via aria-describedby', async () => {
  const user = userEvent.setup();
  render(<ContactForm onSubmit={vi.fn()} />);
  await user.click(screen.getByRole('button', { name: /submit/i }));

  const nameInput = screen.getByLabelText(/name/i);
  const errorId = nameInput.getAttribute('aria-describedby');
  expect(document.getElementById(errorId!)).toHaveTextContent(/name is required/i);
});
```

## Official References

- [WAI Forms Tutorial](https://www.w3.org/WAI/tutorials/forms/)
- [WCAG 3.3 — Input Assistance](https://www.w3.org/TR/WCAG21/#input-assistance)
- [WCAG 1.3.1 — Info and Relationships](https://www.w3.org/TR/WCAG21/#info-and-relationships)
- [MDN — Form Validation and Accessibility](https://developer.mozilla.org/en-US/docs/Learn/Forms/Form_validation)
