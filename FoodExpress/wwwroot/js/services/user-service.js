import { api } from '../api.js';

export const users = {
    update(body) {
        return api.put('/users/me', body, { auth: true });
    },
    changePassword(body) {
        return api.put('/users/me/change-password', body, { auth: true });
    },
    forgotPassword(email) {
        return api.post('/auth/forgot-password', { email });
    },
    resetPassword(body) {
        return api.post('/auth/reset-password', body);
    }
};
