import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import { check, sleep } from 'k6';
import http from 'k6/http';
import { Options } from 'k6/options';

export let options:Options = {
    vus: 50,
    duration: '100s'
  };

interface OrderItemDTO {
  ProductId: string;
  ProductName: string;
  ProductSku: string;
  UnitPrice: number;
  Quantity: number;
  TotalPrice: number;
}

interface CreateOrderDTO {
  OrderId: string;
  OrderDate: string;
  Status: string;
  CustomerId: string;
  CustomerName: string;
  CustomerEmail: string;
  CustomerPhone: string;
  ShippingAddress: string;
  City: string;
  State: string;
  Country: string;
  PostalCode: string;
  EstimatedDeliveryDate?: string;
  PaymentMethod: string;
  TotalAmount: number;
  IsPaymentConfirmed: boolean;
  Items: OrderItemDTO[];
  Notes: string;
}

function generateUUID() {
    // Generates a UUID version 4
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      const r = (Math.random() * 16) | 0;
      const v = c === 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

export default function () {
  const url = 'http://localhost:5064/orders'; // Replace with your endpoint URL

  // Constructing the payload
  const payload: CreateOrderDTO = {
    OrderId: generateUUID(),
    OrderDate: new Date().toISOString(),
    Status: 'Pending',
    CustomerId: generateUUID(),
    CustomerName: 'John Doe',
    CustomerEmail: 'johndoe@example.com',
    CustomerPhone: '+1234567890',
    ShippingAddress: '123 Main Street',
    City: 'Sample City',
    State: 'Sample State',
    Country: 'Sample Country',
    PostalCode: '12345',
    EstimatedDeliveryDate: new Date(new Date().setDate(new Date().getDate() + 7)).toISOString(),
    PaymentMethod: 'Credit Card',
    TotalAmount: 200.0,
    IsPaymentConfirmed: false,
    Items: [
      {
        ProductId:generateUUID(),
        ProductName: 'Sample Product',
        ProductSku: 'SP-001',
        UnitPrice: 50.0,
        Quantity: 4,
        TotalPrice: 200.0,
      },
    ],
    Notes: 'Please deliver between 9 AM and 5 PM',
  };

  // Sending the POST request
  const headers = { 'Content-Type': 'application/json' };
  const response = http.post(url, JSON.stringify(payload), { headers });

  // Validating the response
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  // Simulating user think time
  sleep(randomIntBetween(1, 5));
}
