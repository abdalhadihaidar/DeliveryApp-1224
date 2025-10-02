#!/bin/bash

# Test script to verify authentication token generation
# This script tests the authentication endpoints to ensure they return real tokens instead of "N/A"

BASE_URL="${1:-https://localhost:44356}"
TEST_EMAIL="${2:-test@example.com}"
TEST_PASSWORD="${3:-TestPassword123!}"

echo "🔐 Testing Authentication Token Generation"
echo "Base URL: $BASE_URL"
echo "Test Email: $TEST_EMAIL"
echo ""

# Test 1: Direct OpenIddict Token Endpoint
echo "Test 1: Testing OpenIddict /connect/token endpoint"
TOKEN_RESPONSE=$(curl -s -X POST "$BASE_URL/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "username=$TEST_EMAIL" \
  -d "password=$TEST_PASSWORD" \
  -d "client_id=DeliveryApp_App" \
  -d "client_secret=DeliveryApp_App" \
  -d "scope=DeliveryApp offline_access")

if echo "$TOKEN_RESPONSE" | grep -q '"access_token"' && ! echo "$TOKEN_RESPONSE" | grep -q '"access_token":"N/A"'; then
    echo "✅ OpenIddict token generation: SUCCESS"
    ACCESS_TOKEN=$(echo "$TOKEN_RESPONSE" | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)
    echo "   Access Token: ${ACCESS_TOKEN:0:20}..."
    TOKEN_TYPE=$(echo "$TOKEN_RESPONSE" | grep -o '"token_type":"[^"]*"' | cut -d'"' -f4)
    echo "   Token Type: $TOKEN_TYPE"
    EXPIRES_IN=$(echo "$TOKEN_RESPONSE" | grep -o '"expires_in":[0-9]*' | cut -d':' -f2)
    echo "   Expires In: $EXPIRES_IN seconds"
else
    echo "❌ OpenIddict token generation: FAILED"
    echo "   Response: $TOKEN_RESPONSE"
fi

echo ""

# Test 2: AuthService Login Endpoint
echo "Test 2: Testing AuthService login endpoint"
AUTH_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login/email" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\",\"rememberMe\":false}")

if echo "$AUTH_RESPONSE" | grep -q '"success":true' && echo "$AUTH_RESPONSE" | grep -q '"accessToken"' && ! echo "$AUTH_RESPONSE" | grep -q '"accessToken":"N/A"'; then
    echo "✅ AuthService login: SUCCESS"
    ACCESS_TOKEN=$(echo "$AUTH_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    echo "   Access Token: ${ACCESS_TOKEN:0:20}..."
    REFRESH_TOKEN=$(echo "$AUTH_RESPONSE" | grep -o '"refreshToken":"[^"]*"' | cut -d'"' -f4)
    echo "   Refresh Token: ${REFRESH_TOKEN:0:20}..."
    EXPIRES_AT=$(echo "$AUTH_RESPONSE" | grep -o '"expiresAt":"[^"]*"' | cut -d'"' -f4)
    echo "   Expires At: $EXPIRES_AT"
else
    echo "❌ AuthService login: FAILED"
    echo "   Response: $AUTH_RESPONSE"
fi

echo ""

# Test 3: MobileAuthService Login Endpoint
echo "Test 3: Testing MobileAuthService login endpoint"
MOBILE_RESPONSE=$(curl -s -X POST "$BASE_URL/api/app/mobile-auth/login-with-email" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

if echo "$MOBILE_RESPONSE" | grep -q '"success":true' && echo "$MOBILE_RESPONSE" | grep -q '"accessToken"' && ! echo "$MOBILE_RESPONSE" | grep -q '"accessToken":"N/A"'; then
    echo "✅ MobileAuthService login: SUCCESS"
    ACCESS_TOKEN=$(echo "$MOBILE_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    echo "   Access Token: ${ACCESS_TOKEN:0:20}..."
    REFRESH_TOKEN=$(echo "$MOBILE_RESPONSE" | grep -o '"refreshToken":"[^"]*"' | cut -d'"' -f4)
    echo "   Refresh Token: ${REFRESH_TOKEN:0:20}..."
    EXPIRES_AT=$(echo "$MOBILE_RESPONSE" | grep -o '"expiresAt":"[^"]*"' | cut -d'"' -f4)
    echo "   Expires At: $EXPIRES_AT"
else
    echo "❌ MobileAuthService login: FAILED"
    echo "   Response: $MOBILE_RESPONSE"
fi

echo ""

# Test 4: UserController Login Endpoint (Dashboard)
echo "Test 4: Testing UserController login endpoint (Dashboard)"
USER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/app/user/login" \
  -H "Content-Type: application/json" \
  -d "{\"emailOrPhone\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\",\"rememberMe\":false}")

if echo "$USER_RESPONSE" | grep -q '"token"' && ! echo "$USER_RESPONSE" | grep -q '"token":"N/A"'; then
    echo "✅ UserController login: SUCCESS"
    TOKEN=$(echo "$USER_RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
    echo "   Token: ${TOKEN:0:20}..."
    EXPIRES_IN=$(echo "$USER_RESPONSE" | grep -o '"expiresIn":[0-9]*' | cut -d':' -f2)
    echo "   Expires In: $EXPIRES_IN seconds"
    USER_ID=$(echo "$USER_RESPONSE" | grep -o '"user":{"id":"[^"]*"' | cut -d'"' -f6)
    echo "   User ID: $USER_ID"
else
    echo "❌ UserController login: FAILED"
    echo "   Response: $USER_RESPONSE"
fi

echo ""
echo "🎯 Authentication Token Generation Test Complete!"
echo ""
echo "If all tests show SUCCESS, your token generation fix is working correctly!"
echo "If any tests show FAILED or ERROR, there may be configuration issues."
