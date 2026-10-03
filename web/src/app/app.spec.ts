import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  it('se crea', async () => {
    await TestBed.configureTestingModule({ imports: [App] }).compileComponents();
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
