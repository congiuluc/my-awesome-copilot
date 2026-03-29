// Sample: JPA Entity Listener for audit trail
// Automatically captures entity changes and writes audit entries.

package com.myapp.infrastructure.audit;

import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.persistence.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.MDC;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.stereotype.Component;
import org.springframework.web.context.request.RequestContextHolder;
import org.springframework.web.context.request.ServletRequestAttributes;

import java.time.Instant;
import java.util.Set;
import java.util.UUID;

/**
 * JPA entity listener that creates audit entries on persist, update, and remove.
 * Captures the current user from SecurityContext and correlation ID from MDC.
 */
@Component
public class AuditEntityListener {

    private static final Logger log = LoggerFactory.getLogger(AuditEntityListener.class);
    private static final Set<String> SENSITIVE_FIELDS = Set.of(
            "password", "passwordHash", "token", "secret", "apiKey"
    );
    private static final ObjectMapper objectMapper = new ObjectMapper();

    @PostPersist
    public void onPostPersist(Object entity) {
        createAuditEntry(entity, "CREATE");
    }

    @PostUpdate
    public void onPostUpdate(Object entity) {
        createAuditEntry(entity, "UPDATE");
    }

    @PostRemove
    public void onPostRemove(Object entity) {
        createAuditEntry(entity, "DELETE");
    }

    private void createAuditEntry(Object entity, String action) {
        try {
            AuditEntry entry = AuditEntry.builder()
                    .id(UUID.randomUUID().toString())
                    .timestamp(Instant.now())
                    .userId(getCurrentUserId())
                    .action(action)
                    .entityType(entity.getClass().getSimpleName())
                    .entityId(extractEntityId(entity))
                    .ipAddress(getClientIpAddress())
                    .correlationId(MDC.get("correlationId"))
                    .build();

            // In production, persist via an async event or dedicated repository.
            log.info("Audit: {} {} {} by {}",
                    entry.getAction(),
                    entry.getEntityType(),
                    entry.getEntityId(),
                    entry.getUserId());
        } catch (Exception ex) {
            // Never let audit failures break the primary operation
            log.warn("Failed to create audit entry for {} on {}",
                    action, entity.getClass().getSimpleName(), ex);
        }
    }

    private String getCurrentUserId() {
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        if (auth != null && auth.isAuthenticated()) {
            return auth.getName();
        }
        return "system";
    }

    private String getClientIpAddress() {
        try {
            var requestAttributes = (ServletRequestAttributes) RequestContextHolder
                    .getRequestAttributes();
            if (requestAttributes != null) {
                return requestAttributes.getRequest().getRemoteAddr();
            }
        } catch (Exception ignored) {
            // Not in a web context
        }
        return null;
    }

    private String extractEntityId(Object entity) {
        try {
            var idField = entity.getClass().getDeclaredField("id");
            idField.setAccessible(true);
            var value = idField.get(entity);
            return value != null ? value.toString() : "unknown";
        } catch (Exception ex) {
            return "unknown";
        }
    }
}
