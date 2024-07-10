import axios from "axios";
import FormData from 'form-data';
import { createReadStream, readdirSync, statSync } from 'fs';
import { join } from 'path';
import { fileURLToPath } from 'url';

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
//const HOST_URL = 'https://100.29.48.143';
//const HOST_URL = 'http://localhost:5266';
const HOST_URL = 'http://localhost:8000';
const __dirname = fileURLToPath(new URL('.', import.meta.url));

// Configuration
const IMAGE_PATH: string = join(__dirname, './assets/image.png');
const API_URL = `${HOST_URL}/api/v1/profile`; // Replace with your API URL
const TEMP_FOLDER: string = join(__dirname, './temp');

async function uploadImages() {
  try {
    // Create a new form data instance
    const form = new FormData();

    // Read all files in the TEMP_FOLDER
    const files = readdirSync(TEMP_FOLDER);

    form.append('profileImg', createReadStream(IMAGE_PATH));
    // Filter and append image files to the form data
    //files.forEach(file => {
    //  const filePath = join(TEMP_FOLDER, file);
    //  if (statSync(filePath).isFile() && /\.(png|jpe?g|gif)$/i.test(file)) {
    //    form.append('profileImg', createReadStream(filePath));
    //  }
    //});

    //const login = await axios.post(`${HOST_URL}/api/auth/login`, {
    //    email: 'admin@example.com',
    //    password: 'Password123!'
    //});
    //
    //const authToken = login.data.token;
    //
    //// Make the POST request
    const response = await axios.put(API_URL, form, {
      headers: {
		Cookie: "auth_session=smqicojeel6bg4tasxfpo5vzm23yxw2hzgxmsyjn; HttpOnly; Max-Age=2592000; Path=/; SameSite=Lax",
        ...form.getHeaders(),
        //'Authorization': `Bearer ${authToken}`
      }
    });

    console.log('Upload successful!');
    console.log('Uploaded image URLs:', response.data);
  } catch (error: any) {
    console.log(error);
    console.error('Error uploading images:', error.response ? error.response.data : error.message);
  }
}

uploadImages();
