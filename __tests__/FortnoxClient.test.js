const FortnoxClient = require('../src/FortnoxClient');
const axios = require('axios');

// Mock axios
jest.mock('axios');

describe('FortnoxClient', () => {
  let client;
  const mockAccessToken = 'test-access-token-123';
  const mockBaseURL = 'https://api.fortnox.se/3';

  beforeEach(() => {
    // Reset mocks before each test
    jest.clearAllMocks();
    
    // Setup axios.create mock
    axios.create.mockReturnValue({
      get: jest.fn(),
      post: jest.fn(),
      put: jest.fn(),
      delete: jest.fn()
    });
  });

  describe('Constructor', () => {
    test('should throw error when accessToken is not provided', () => {
      expect(() => new FortnoxClient()).toThrow('Access token is required');
      expect(() => new FortnoxClient({})).toThrow('Access token is required');
    });

    test('should create client with required accessToken', () => {
      client = new FortnoxClient({ accessToken: mockAccessToken });
      
      expect(client.accessToken).toBe(mockAccessToken);
      expect(client.baseURL).toBe(mockBaseURL);
      expect(axios.create).toHaveBeenCalledWith({
        baseURL: mockBaseURL,
        headers: {
          'Access-Token': mockAccessToken,
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        timeout: 30000
      });
    });

    test('should accept custom baseURL', () => {
      const customBaseURL = 'https://custom.api.com';
      client = new FortnoxClient({ 
        accessToken: mockAccessToken,
        baseURL: customBaseURL 
      });
      
      expect(client.baseURL).toBe(customBaseURL);
      expect(axios.create).toHaveBeenCalledWith(
        expect.objectContaining({ baseURL: customBaseURL })
      );
    });

    test('should accept custom timeout', () => {
      const customTimeout = 60000;
      client = new FortnoxClient({ 
        accessToken: mockAccessToken,
        timeout: customTimeout 
      });
      
      expect(axios.create).toHaveBeenCalledWith(
        expect.objectContaining({ timeout: customTimeout })
      );
    });
  });

  describe('GET requests', () => {
    beforeEach(() => {
      client = new FortnoxClient({ accessToken: mockAccessToken });
    });

    test('should successfully perform GET request', async () => {
      const mockData = { customers: [{ id: 1, name: 'Test Customer' }] };
      client.client.get.mockResolvedValue({ data: mockData });

      const result = await client.get('/customers');

      expect(client.client.get).toHaveBeenCalledWith('/customers', { params: {} });
      expect(result).toEqual(mockData);
    });

    test('should pass query parameters in GET request', async () => {
      const mockData = { customers: [] };
      const params = { limit: 10, offset: 0 };
      client.client.get.mockResolvedValue({ data: mockData });

      await client.get('/customers', params);

      expect(client.client.get).toHaveBeenCalledWith('/customers', { params });
    });

    test('should handle GET request errors', async () => {
      const mockError = {
        response: {
          status: 404,
          statusText: 'Not Found',
          data: { message: 'Resource not found' }
        }
      };
      client.client.get.mockRejectedValue(mockError);

      await expect(client.get('/nonexistent')).rejects.toThrow('Resource not found');
    });
  });

  describe('POST requests', () => {
    beforeEach(() => {
      client = new FortnoxClient({ accessToken: mockAccessToken });
    });

    test('should successfully perform POST request', async () => {
      const mockRequestData = { name: 'New Customer', email: 'test@example.com' };
      const mockResponseData = { id: 123, ...mockRequestData };
      client.client.post.mockResolvedValue({ data: mockResponseData });

      const result = await client.post('/customers', mockRequestData);

      expect(client.client.post).toHaveBeenCalledWith('/customers', mockRequestData);
      expect(result).toEqual(mockResponseData);
    });

    test('should handle POST request with empty data', async () => {
      const mockResponseData = { success: true };
      client.client.post.mockResolvedValue({ data: mockResponseData });

      const result = await client.post('/endpoint');

      expect(client.client.post).toHaveBeenCalledWith('/endpoint', {});
      expect(result).toEqual(mockResponseData);
    });

    test('should handle POST request errors', async () => {
      const mockError = {
        response: {
          status: 400,
          statusText: 'Bad Request',
          data: { message: 'Invalid data' }
        }
      };
      client.client.post.mockRejectedValue(mockError);

      await expect(client.post('/customers', {})).rejects.toThrow('Invalid data');
    });
  });

  describe('PUT requests', () => {
    beforeEach(() => {
      client = new FortnoxClient({ accessToken: mockAccessToken });
    });

    test('should successfully perform PUT request', async () => {
      const mockRequestData = { name: 'Updated Customer' };
      const mockResponseData = { id: 123, ...mockRequestData };
      client.client.put.mockResolvedValue({ data: mockResponseData });

      const result = await client.put('/customers/123', mockRequestData);

      expect(client.client.put).toHaveBeenCalledWith('/customers/123', mockRequestData);
      expect(result).toEqual(mockResponseData);
    });

    test('should handle PUT request errors', async () => {
      const mockError = {
        response: {
          status: 403,
          statusText: 'Forbidden',
          data: { message: 'Access denied' }
        }
      };
      client.client.put.mockRejectedValue(mockError);

      await expect(client.put('/customers/123', {})).rejects.toThrow('Access denied');
    });
  });

  describe('DELETE requests', () => {
    beforeEach(() => {
      client = new FortnoxClient({ accessToken: mockAccessToken });
    });

    test('should successfully perform DELETE request', async () => {
      const mockResponseData = { success: true };
      client.client.delete.mockResolvedValue({ data: mockResponseData });

      const result = await client.delete('/customers/123');

      expect(client.client.delete).toHaveBeenCalledWith('/customers/123');
      expect(result).toEqual(mockResponseData);
    });

    test('should handle DELETE request errors', async () => {
      const mockError = {
        response: {
          status: 404,
          statusText: 'Not Found',
          data: { message: 'Customer not found' }
        }
      };
      client.client.delete.mockRejectedValue(mockError);

      await expect(client.delete('/customers/999')).rejects.toThrow('Customer not found');
    });
  });

  describe('Error handling', () => {
    beforeEach(() => {
      client = new FortnoxClient({ accessToken: mockAccessToken });
    });

    test('should handle server error response', async () => {
      const mockError = {
        response: {
          status: 500,
          statusText: 'Internal Server Error',
          data: { message: 'Server error' }
        }
      };
      client.client.get.mockRejectedValue(mockError);

      try {
        await client.get('/endpoint');
      } catch (error) {
        expect(error.message).toBe('Server error');
        expect(error.status).toBe(500);
        expect(error.data).toEqual(mockError.response.data);
      }
    });

    test('should handle error response without message', async () => {
      const mockError = {
        response: {
          status: 401,
          statusText: 'Unauthorized',
          data: {}
        }
      };
      client.client.get.mockRejectedValue(mockError);

      await expect(client.get('/endpoint')).rejects.toThrow('Unauthorized');
    });

    test('should handle network errors', async () => {
      const mockError = {
        request: {},
        message: 'Network Error'
      };
      client.client.get.mockRejectedValue(mockError);

      try {
        await client.get('/endpoint');
      } catch (error) {
        expect(error.message).toBe('Network error: No response from server');
        expect(error.isNetworkError).toBe(true);
      }
    });

    test('should handle request setup errors', async () => {
      const mockError = new Error('Request configuration error');
      client.client.get.mockRejectedValue(mockError);

      await expect(client.get('/endpoint')).rejects.toThrow('Request configuration error');
    });
  });

  describe('Connection testing', () => {
    beforeEach(() => {
      client = new FortnoxClient({ accessToken: mockAccessToken });
    });

    test('should return true when connection test succeeds', async () => {
      const mockData = { CompanyInformation: { CompanyName: 'Test Company' } };
      client.client.get.mockResolvedValue({ data: mockData });

      const result = await client.testConnection();

      expect(result).toBe(true);
      expect(client.client.get).toHaveBeenCalledWith('/companyinformation', { params: {} });
    });

    test('should return false when connection test fails', async () => {
      const mockError = {
        response: {
          status: 401,
          statusText: 'Unauthorized',
          data: { message: 'Invalid token' }
        }
      };
      client.client.get.mockRejectedValue(mockError);

      const result = await client.testConnection();

      expect(result).toBe(false);
    });
  });
});
