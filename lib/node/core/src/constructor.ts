export type constructor<T> = {
    new(...args: any[]): T,
    name: string;
}
