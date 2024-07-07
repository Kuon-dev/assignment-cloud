import axios from "axios";
import FormData from 'form-data';
import { createReadStream } from 'fs';
import { join } from 'path';
import { fileURLToPath } from 'url';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
// Configuration
const API_URL = 'http://localhost:5266/api/property/upload-images'; // Replace with your API URL
const IMAGE_PATH: string = join(__dirname, './assets/image.png');
const AUTH_TOKEN = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjU2MzEzNzM4LWFhNjYtNGY0NS1hZmNlLWIwYjc0ZjlhYmNjMSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InJrM2JvZThvYW5AZXhhbXBsZS5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJPd25lciIsImp0aSI6ImNiYzVmOWFlLTRjOGUtNDJjMC1hMGNhLTEzZGQ3YTkyMTY3NSIsImV4cCI6MTcyMDQzMzMzNSwiaXNzIjoia3VvbiIsImF1ZCI6Imt1b24ifQ.GBRtbeKsWIF4oHNCuH3o3gyu-GuVGcx8ZX5w6D7qjGc'; // Replace with a valid auth token

async function uploadImage() {
  try {
    // Create a new form data instance
    const form = new FormData();

    // Append the file to the form data
    form.append('images', createReadStream(IMAGE_PATH));

    // Make the POST request
    const response = await axios.post(API_URL, form, {
      headers: {
        ...form.getHeaders(),
        'Authorization': `Bearer ${AUTH_TOKEN}`
      }
    });

    console.log('Upload successful!');
    console.log('Uploaded image URL:', response.data[0]);
  } catch (error: any) {
    console.log(error)
    console.error('Error uploading image:', error.response ? error.response.data : error.message);
  }
}

uploadImage();
