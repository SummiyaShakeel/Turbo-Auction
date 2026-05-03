#🏎️ Turbo-Auction: The Founders Circuit Edition
A Distributed Real-Time Bidding System for Luxury Vehicles

##📌 Project Overview
Turbo-Auction is a Parallel and Distributed Computing (PDC) project developed for the BSCS (AI) program. It utilizes a Centralized Server (Master Node) to orchestrate real-time auctions between multiple Client Nodes (Bidders) over a distributed network using TCP/IP Sockets.

The system is designed with a "Mafia-style" cinematic aesthetic, featuring high-contrast dark themes, Neon Cyan, and Emerald Green visuals.

##🏗️ Core System Architecture
While the repository contains various supporting files, the entire distributed system is powered by two primary functional units:

###1. AuctionServer (The Master Node)
The Brain: Developed by Summiya.

Role: Handles the central socket listener, manages the "Gatekeeper" authorization for incoming teammates, and synchronizes the global 30-second auction clock across all parallel nodes.

###2. TurboAuctionWPF (The Client Application)
The Interface: Developed by Areeba, Zunaina, and Laraib.

Role: The frontend bidding dashboard that connects to the Master Node. It handles real-time data staging (images and specs) and executes parallel threading to ensure the UI remains responsive during high-speed bidding wars.

##👥 The Team & Contributions
Summiya Shakeel (Founder & Lead): System Architecture, Master Logic, & Network Orchestration.

Zunaina: Networking Layer, Client-Server Handshake, & JSON Data Streaming.

Laraib: Data Modeling, Car Dataset (JSON), & Serialization Helpers.

Areeba: UI/UX Design (WPF), Parallelism (Threading), & Timer Logic.

##🛠️ Tech Stack
Language: C# (.NET 8)

Communication: System.Net.Sockets (TCP/IP)

Design Framework: WPF (Windows Presentation Foundation)

Data Protocol: JSON (System.Text.Json)
