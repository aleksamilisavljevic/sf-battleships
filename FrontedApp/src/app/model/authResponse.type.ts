import { User } from '../model/user.type';
export type AuthResponse = {
    user: User;
    token: string;
}