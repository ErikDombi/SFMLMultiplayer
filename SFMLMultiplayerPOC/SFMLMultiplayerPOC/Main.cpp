#include <iostream>
#include <SFML/Graphics.hpp>
#include <SFML/Network.hpp>
#include "json.hpp"
#include <vector>
#include <string.h>
#include <algorithm>
#include "SFMLUtils.h"

sf::RenderWindow window(sf::VideoMode(1600, 800), "SFML Multiplayer POC");
SFMLUtils utils;

void eraseSubStr(std::string & mainStr, const std::string & toErase)
{
	// Search for the substring in string
	size_t pos = mainStr.find(toErase);

	if (pos != std::string::npos)
	{
		// If found then erase it from string
		mainStr.erase(pos, toErase.length());
	}
}

int main() {
	bool focus = false;
	window.setFramerateLimit(60);
	sf::CircleShape shape(100.f);
	shape.setFillColor(sf::Color::Green);

	sf::CircleShape enemy(100.f);
	enemy.setFillColor(sf::Color::Red);


	sf::TcpSocket socket;
	socket.connect("99.243.157.100", 6654);

	nlohmann::json recieveJson;
	nlohmann::json sendJson;

	while (window.isOpen())
	{

#pragma region NETWORKING
		std::string::size_type sz;
		sendJson["x"] = shape.getPosition().x;
		sendJson["y"] = shape.getPosition().y;
		std::string message = sendJson.dump();
		socket.send(message.c_str(), message.size() + 1);

		char buffer[1024];
		std::size_t recieved = 0;
		socket.receive(buffer, sizeof(buffer), recieved);
		std::string bufferString(buffer);
		int jsonLength = std::stoi(bufferString.substr(0, 5), &sz);
		auto x = bufferString.substr(5, jsonLength);
		recieveJson = nlohmann::json::parse(x);
		if (recieveJson["playerCount"] > 0) {
			for (const auto player : recieveJson["players"]) {
				auto xPos = player["x"];
				auto yPos = player["y"];
				std::cout << xPos << " " << yPos << std::endl;
				enemy.setPosition(xPos, yPos);
			}
		}
	
#pragma endregion



		sf::Event event;
		while (window.pollEvent(event))
		{
			if (event.type == sf::Event::Closed)
				window.close();
			if (event.type == sf::Event::GainedFocus) focus = true;
			if (event.type == sf::Event::LostFocus) focus = false;
		}

		if (sf::Keyboard::isKeyPressed(sf::Keyboard::A) && focus) {
			shape.move(-1, 0);
		}

		if (sf::Keyboard::isKeyPressed(sf::Keyboard::D) && focus) {
			shape.move(1, 0);
		}

		if (sf::Keyboard::isKeyPressed(sf::Keyboard::W) && focus) {
			shape.move(0, -1);
		}

		if (sf::Keyboard::isKeyPressed(sf::Keyboard::S) && focus) {
			shape.move(0, 1);
		}

		window.clear();
		window.draw(shape);
		window.draw(enemy);
		window.display();
	}

	return 0;
}