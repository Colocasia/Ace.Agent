package main

import "fmt"

type Greeter struct {
    name string
}

func (g *Greeter) Greet() {
    fmt.Printf("Hello, %s from Go!\n", g.name)
}

func main() {
    greeter := &Greeter{name: "World"}
    greeter.Greet()
}