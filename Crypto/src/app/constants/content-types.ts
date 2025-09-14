export interface ContentType {
  value: string;
  name: string;
}

export const CONTENT_TYPES: ContentType[] = [
  { value: 'Market News', name: 'Market News & Analysis' },
  { value: 'Charts', name: 'Price Charts & Data' },
  { value: 'Social', name: 'Social Sentiment' },
  { value: 'Fun', name: 'Memes & Community' }
];
