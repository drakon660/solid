package main

import "fmt"

type IArgument interface {
	GetValue() string
}

type Argument struct {
	Value string
}

func (a Argument) GetValue() string {
	return a.Value
}

type ServiceArgument struct {
	Argument
	ServiceID string
}

func (sa ServiceArgument) GetServiceID() string {
	return sa.ServiceID
}

type Service struct{}

func (s Service) Action(arg IArgument) {
	fmt.Printf(arg.GetValue())
}

func main() {
	service := Service{}

	baseArg := Argument{Value: "base-argument"}

	// Create specific type (subtype)
	specificArg := ServiceArgument{
		Argument:  Argument{Value: "service-argument"},
		ServiceID: "service-123",
	}

	service.Action(baseArg)
	service.Action(specificArg)
}
