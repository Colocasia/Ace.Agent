interface Greeter {
    name: string;
    greet(): void;
}

class HelloWorld implements Greeter {
    name: string;
    
    constructor(name: string) {
        this.name = name;
    }
    
    greet(): void {
        console.log(`Hello, ${this.name} from TypeScript!`);
    }
}

const greeter = new HelloWorld("World");
greeter.greet();