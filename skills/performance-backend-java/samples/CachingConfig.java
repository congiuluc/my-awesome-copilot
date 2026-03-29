// Sample: Spring Cache configuration with Caffeine and cache annotations

package com.myapp.infrastructure.config;

import com.github.benmanes.caffeine.cache.Caffeine;
import org.springframework.cache.CacheManager;
import org.springframework.cache.annotation.EnableCaching;
import org.springframework.cache.caffeine.CaffeineCacheManager;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.time.Duration;

/**
 * Configures in-memory caching with Caffeine.
 * For distributed caching, replace with RedisCacheManager.
 */
@Configuration
@EnableCaching
public class CachingConfig {

    @Bean
    public CacheManager cacheManager() {
        var caffeine = Caffeine.newBuilder()
            .maximumSize(1000)
            .expireAfterWrite(Duration.ofMinutes(10))
            .recordStats();

        var manager = new CaffeineCacheManager("products", "categories", "users");
        manager.setCaffeine(caffeine);
        return manager;
    }
}
