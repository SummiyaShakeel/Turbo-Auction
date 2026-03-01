# 🏎️ Turbo-Auction
**A Distributed Real-Time Bidding System for Luxury Vehicles**

## 📌 Project Overview
Turbo-Auction is a 6th-semester Parallel and Distributed Computing (PDC) project. It utilizes a **Centralized Server (Master Node)** to orchestrate real-time auctions between multiple **Client Nodes (Bidders)** over a local network using TCP/IP Sockets.

## 👥 The Team & Roles
* **Summiya (Team Lead):** Server Architecture, Master Logic, & Repository Management.
* **Zunaina:** Networking Layer, Client-Server Handshake, & JSON Data Streaming.
* **Laraib:** Data Modeling, Car Dataset (JSON), & Serialization Helpers.
* **Areeba:** UI/UX Design (WPF), Parallelism (Threading), & Timer Logic.

## 🛠️ Tech Stack
* **Language:** C# (.NET 8)
* **Communication:** System.Net.Sockets (TCP/IP)
* **UI:** WPF (Windows Presentation Foundation)
* **Data Format:** JSON (System.Text.Json)

## 📁 Repository Structure
- `/AuctionServer`: The Master Node logic (Summiya).
- `/AuctionClient`: The WPF Bidding Application (Areeba & Zunaina).
- `/Shared`: Shared models and car datasets (Laraib).
