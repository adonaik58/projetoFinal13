import { API_BASE } from "../utils/baseAPI.js";

const userService = {
  getUsers: () => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/user/data`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  deleteUser: (id) => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/user/delete?id=${id}`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  newUser: (formData) => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/user/new`, {
        method: "POST",
        body: formData,
      })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  getUserById: (ID) => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/user/data?id=${ID}`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  updateUser: (formData, ID) => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/user/update?id=${ID}`, {
        method: "POST",
        body: formData,
      })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
};

export default userService;
