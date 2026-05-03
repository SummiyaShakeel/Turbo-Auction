🌐 Turbo Auction | Master Node (Server)
The Central Intelligence for Autonomous Bidding Systems.

This repository contains the Auction Server, the core engine of the Turbo Auction ecosystem. It manages real-time socket connections, handles bid validation, and orchestrates data staging for distributed clients.

⚡ Key Features
TCP/IP Socket Architecture: Built for high-speed, low-latency communication between the Admin and Bidders.

Authorization Protocol: Integrated "Gatekeeper" logic to approve or deny incoming connection requests manually.

Live Broadcast Engine: Pushes vehicle technical specs and imagery to all connected nodes simultaneously.

Automated Countdown: Syncs a 30-second auction clock across the network, resetting dynamically with every valid bid.

🛠️ Tech Stack
Language: C# / .NET 8.0

Networking: System.Net.Sockets

Framework: WPF (Windows Presentation Foundation)

Data Handling: Newtonsoft.Json for packet serialization

🚀 Getting Started
1. Prerequisites
Windows OS

.NET 8.0 Runtime

A stable internet connection (for remote testing)

2. Local Setup
Clone this repository.

Open the solution in Visual Studio 2022.

Build the project to generate the AuctionServer.exe.

Run the application. The server will automatically begin listening on Port 8080.

3. Remote Access (The Systempreneur Way)
To connect with teammates across different networks without using external software on their end, use Port Forwarding on your router:

Internal IP: Your local IPv4 address.

Port: 8080.

Protocol: TCP.

🛡️ Security & Reliability
This server utilizes a Manual Authorization flow. No client can access the auction dashboard or see staged data until the Admin explicitly grants access. This prevents unauthorized data scraping and ensures a controlled testing environment.

💎 About the Project
Developed as part of The Founders Circuit, this system is a blueprint for Agentic AI leverage and autonomous software generation.

Lead Developer: Summiya Shakeel
Brand: The Founders Circuit

📄 License
This project is for academic and portfolio demonstration purposes. All cinematic branding and technical blueprints are property of the author.
