/**
 * Validation class to represent the result of a validation process.
 */
export default class Validation {
    success: boolean;
    message: string;

    /**
     * Creates a new instance of Validation.
     * @param success If the validation was successful or not
     * @param message The message associated with the validation result
     */
    constructor(success: boolean = true, message: string = '') {
        this.success = success;
        this.message = message;
    }
}
