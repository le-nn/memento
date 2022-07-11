export abstract class Message<TPayload = null | {} | void> {
    readonly type: string;

    constructor(public readonly payload: TPayload) {
        this.type = (this.constructor as any).message || this.constructor.name;
    }

    get comparer() {
        return this.constructor;
    }
}