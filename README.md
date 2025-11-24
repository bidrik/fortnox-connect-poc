# fortnox-connect-poc

A proof of concept for connecting to the Fortnox API with comprehensive unit tests.

## Features

- Simple and clean API client for Fortnox
- Support for GET, POST, PUT, and DELETE requests
- Comprehensive error handling
- Connection testing
- Full unit test coverage

## Installation

```bash
npm install
```

## Usage

```javascript
const FortnoxClient = require('./src/FortnoxClient');

// Initialize client
const client = new FortnoxClient({
  accessToken: 'your-fortnox-access-token'
});

// Make API requests
const customers = await client.get('/customers');
const newCustomer = await client.post('/customers', { name: 'John Doe' });
const updated = await client.put('/customers/123', { name: 'Jane Doe' });
await client.delete('/customers/123');

// Test connection
const isConnected = await client.testConnection();
```

## Configuration Options

- `accessToken` (required): Your Fortnox API access token
- `baseURL` (optional): Custom base URL (default: 'https://api.fortnox.se/3')
- `timeout` (optional): Request timeout in milliseconds (default: 30000)

## Running Tests

```bash
# Run all tests
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage report
npm run test:coverage
```

## Test Coverage

The project includes comprehensive unit tests covering:
- Client initialization and configuration
- GET request handling
- POST request handling
- PUT request handling
- DELETE request handling
- Error handling for various scenarios
- Network error handling
- Connection testing

Current coverage: 100% statements, 88.23% branches, 100% functions