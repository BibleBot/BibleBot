package main

import (
	"fmt"
	"os"
	"os/signal"
	"syscall"

	"github.com/bwmarrin/discordgo"
	"gopkg.in/go-ini/ini.v1"
)

func main() {
	config, err := ini.Load("config.ini")
	if err != nil {
		fmt.Println("[err] couldn't load config.ini")
		os.Exit(1)
	}

	discord, err := discordgo.New("Bot " + config.Section("BibleBot").Key("token").String())
	if err != nil {
		fmt.Println("[err] couldn't start discord session")
		os.Exit(2)
	}

	discord.AddHandler(messageCreateHandler)

	err = discord.Open()
	if err != nil {
		fmt.Println("[err] couldn't open a connection")
		os.Exit(3)
	}

	fmt.Printf("[info] BibleBot v%s by Seraphim R.P. (vypr)\n", config.Section("meta").Key("version").String())

	sc := make(chan os.Signal, 1)
	signal.Notify(sc, syscall.SIGINT, syscall.SIGTERM, os.Interrupt, os.Kill)
	<-sc

	fmt.Println("[info] received closing signal, goodbye")
	discord.Close()
}

func messageCreateHandler(s *discordgo.Session, m *discordgo.MessageCreate) {
	if m.Author.ID == s.State.User.ID {
		return
	}

	if m.Content == "+ping" {
		s.ChannelMessageSend(m.ChannelID, "pong")
	}
}
