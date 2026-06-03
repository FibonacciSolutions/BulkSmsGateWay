const { Client, LocalAuth } = require('whatsapp-web.js');
const qrcode = require('qrcode-terminal');
const express = require('express');

const app = express();
app.use(express.json());

// Initialize WhatsApp client with local session caching
const client = new Client({
    authStrategy: new LocalAuth(),
    puppeteer: {
        args: ['--no-sandbox', '--disable-setuid-sandbox']
    }
});

// 1. Generate the QR Code in the terminal for authentication
client.on('qr', (qr) => {
    console.log('\n--- SCAN THIS QR CODE WITH YOUR WHATSAPP TO LINK ENGINE ---');
    qrcode.generate(qr, { small: true });
});

client.on('ready', () => {
    console.log('\n🚀 OmniRoute WhatsApp Worker Node is ONLINE and Authenticated!');
});

// 2. HTTP Endpoint for your .NET Core API to trigger dispatches
app.post('/api/worker/send-whatsapp', async (req, res) => {
    const { to, message } = req.body;

    if (!to || !message) {
        return res.status(404).json({ error: 'Missing parameters: "to" and "message" are required.' });
    }

    try {
        // Format Nepali numbers to international standard (98XXXXXXXX to 97798XXXXXXXX@c.us)
        let formattedNumber = to.replace(/[^0-9]/g, '');
        if (!formattedNumber.startsWith('977')) {
            formattedNumber = '977' + formattedNumber;
        }
        const chatId = `${formattedNumber}@c.us`;

        // Send the message natively using your linked phone session
        await client.sendMessage(chatId, message);
        
        console.log(`Message successfully dispatched to: ${chatId}`);
        return res.json({ success: true, status: 'DeliveredToNetwork' });
    } catch (error) {
        console.error('Failed to send WhatsApp message via worker:', error);
        return res.status(500).json({ error: 'Internal worker transmission failure.' });
    }
});

// Fire up the microservice on port 5001
client.initialize();
app.listen(5001, () => {
    console.log('WhatsApp Worker API listening on http://localhost:5001');
});