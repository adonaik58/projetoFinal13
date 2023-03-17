import { API_BASE } from "../utils/baseAPI.js";

const spaceService = {
  getSpaces: () => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/spaces/all`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  carInSpace: (formData) => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/space/car/new`, {
        method: "POST",
        body: formData,
      })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  newSpace: () => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/space/new`)
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  getSpaceById: (ID) => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/space/data?id=${ID}`, { method: "GET" })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
  updateSpace: (formData, ID) => {
    return new Promise((resolve, reject) => {
      fetch(`${API_BASE}/space/update?id=${ID}`, {
        method: "POST",
        body: formData,
      })
        .then((response) => resolve(response.json()))
        .catch((err) => reject(err));
    });
  },
};

export default spaceService;
