/**
 * Frontend Security Sample
 *
 * Demonstrates XSS prevention, safe URL handling, file upload validation,
 * and HTML sanitization patterns for React applications.
 *
 * Official references:
 * - https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html
 * - https://github.com/cure53/DOMPurify
 */

// =============================================================================
// Safe HTML Rendering with DOMPurify
// =============================================================================

import DOMPurify from 'dompurify';

interface SafeHtmlProps {
  html: string;
  className?: string;
}

/**
 * Renders user-provided HTML safely by sanitizing with DOMPurify.
 * Use only when `dangerouslySetInnerHTML` is truly required (e.g., CMS content).
 * Prefer plain text rendering whenever possible.
 */
export const SafeHtml = ({ html, className }: SafeHtmlProps) => (
  <div
    className={className}
    dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(html) }}
  />
);

// =============================================================================
// Safe URL Construction
// =============================================================================

/**
 * Build a URL with user-provided query parameters, encoding all values
 * to prevent injection.
 */
export const buildSafeUrl = (
  base: string,
  params: Record<string, string>,
): string => {
  const url = new URL(base, window.location.origin);
  for (const [key, value] of Object.entries(params)) {
    url.searchParams.set(key, value);
  }
  return url.toString();
};

/**
 * Validate that a redirect URL is same-origin to prevent open redirects.
 */
export const isSafeRedirect = (url: string): boolean => {
  try {
    const parsed = new URL(url, window.location.origin);
    return parsed.origin === window.location.origin;
  } catch {
    return false;
  }
};

// =============================================================================
// File Upload Validation
// =============================================================================

const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB

interface FileValidationResult {
  valid: boolean;
  error?: string;
}

/**
 * Validate a file before upload — check type and size on the client.
 * MUST also validate on the server; client validation is for UX only.
 */
export const validateImageFile = (file: File): FileValidationResult => {
  if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
    return {
      valid: false,
      error: `File type "${file.type}" is not allowed. Use JPEG, PNG, or WebP.`,
    };
  }

  if (file.size > MAX_FILE_SIZE) {
    return {
      valid: false,
      error: `File size ${(file.size / 1024 / 1024).toFixed(1)} MB exceeds the 5 MB limit.`,
    };
  }

  return { valid: true };
};

// =============================================================================
// Secure Image Upload Component
// =============================================================================

import { useState, type ChangeEvent } from 'react';

interface ImageUploadProps {
  onFileSelect: (file: File) => void;
  maxSizeMb?: number;
}

export const ImageUpload = ({
  onFileSelect,
  maxSizeMb = 5,
}: ImageUploadProps) => {
  const [error, setError] = useState<string>();

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const result = validateImageFile(file);
    if (!result.valid) {
      setError(result.error);
      return;
    }

    setError(undefined);
    onFileSelect(file);
  };

  return (
    <div>
      <label htmlFor="image-upload" className="block text-sm font-medium">
        Upload image
      </label>
      <input
        id="image-upload"
        type="file"
        accept={ALLOWED_IMAGE_TYPES.join(',')}
        onChange={handleChange}
        aria-describedby={error ? 'upload-error' : undefined}
        className="mt-1 block w-full text-sm"
      />
      {error && (
        <p id="upload-error" role="alert" className="mt-1 text-sm text-red-600">
          {error}
        </p>
      )}
      <p className="mt-1 text-xs text-gray-500">
        JPEG, PNG, or WebP. Max {maxSizeMb} MB.
      </p>
    </div>
  );
};
