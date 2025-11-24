const axios = require('axios');

/**
 * FortnoxClient - A simple client for connecting to the Fortnox API
 */
class FortnoxClient {
  /**
   * Creates a FortnoxClient instance
   * @param {Object} config - Configuration object
   * @param {string} config.accessToken - Fortnox API access token
   * @param {string} config.baseURL - Base URL for Fortnox API (default: https://api.fortnox.se/3)
   */
  constructor(config = {}) {
    if (!config.accessToken) {
      throw new Error('Access token is required');
    }

    this.accessToken = config.accessToken;
    this.baseURL = config.baseURL || 'https://api.fortnox.se/3';
    
    this.client = axios.create({
      baseURL: this.baseURL,
      headers: {
        'Access-Token': this.accessToken,
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      timeout: config.timeout || 30000
    });
  }

  /**
   * Performs a GET request
   * @param {string} endpoint - API endpoint
   * @param {Object} params - Query parameters
   * @returns {Promise<Object>} Response data
   */
  async get(endpoint, params = {}) {
    try {
      const response = await this.client.get(endpoint, { params });
      return response.data;
    } catch (error) {
      throw this._handleError(error);
    }
  }

  /**
   * Performs a POST request
   * @param {string} endpoint - API endpoint
   * @param {Object} data - Request body data
   * @returns {Promise<Object>} Response data
   */
  async post(endpoint, data = {}) {
    try {
      const response = await this.client.post(endpoint, data);
      return response.data;
    } catch (error) {
      throw this._handleError(error);
    }
  }

  /**
   * Performs a PUT request
   * @param {string} endpoint - API endpoint
   * @param {Object} data - Request body data
   * @returns {Promise<Object>} Response data
   */
  async put(endpoint, data = {}) {
    try {
      const response = await this.client.put(endpoint, data);
      return response.data;
    } catch (error) {
      throw this._handleError(error);
    }
  }

  /**
   * Performs a DELETE request
   * @param {string} endpoint - API endpoint
   * @returns {Promise<Object>} Response data
   */
  async delete(endpoint) {
    try {
      const response = await this.client.delete(endpoint);
      return response.data;
    } catch (error) {
      throw this._handleError(error);
    }
  }

  /**
   * Handles API errors
   * @private
   * @param {Error} error - Axios error object
   * @returns {Error} Formatted error
   */
  _handleError(error) {
    if (error.response) {
      // Server responded with error status
      const apiError = new Error(
        error.response.data?.message || 
        error.response.statusText || 
        'API request failed'
      );
      apiError.status = error.response.status;
      apiError.data = error.response.data;
      return apiError;
    } else if (error.request) {
      // Request was made but no response received
      const networkError = new Error('Network error: No response from server');
      networkError.isNetworkError = true;
      return networkError;
    } else {
      // Error in request setup
      return error;
    }
  }

  /**
   * Tests the connection by fetching company information
   * @returns {Promise<boolean>} True if connection is successful
   */
  async testConnection() {
    try {
      await this.get('/companyinformation');
      return true;
    } catch (error) {
      return false;
    }
  }
}

module.exports = FortnoxClient;
